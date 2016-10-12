using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.TestUtils.Awaitable;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class HealthSamplerTests
    {
        private readonly HealthSampler _sampler;
        private readonly Mock<IEndpointHealthUpdateListener> _healthUpdateListener;
        private readonly Mock<IHealthMonitor> _monitor;
        private readonly MonitorableEndpoint _endpoint;
        private readonly Mock<ITimeCoordinator> _timeCoordinator;
        private readonly AwaitableFactory _awaitableFactory;

        private readonly TimeSpan _shortTimeOut = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _healthyResponseTimeLimit = TimeSpan.FromSeconds(3);
        private readonly TimeSpan _failureTimeOut = TimeSpan.FromSeconds(20);
        private readonly DateTime _utcNow = DateTime.UtcNow;

        public HealthSamplerTests()
        {
            var settings = SetupSettings();
            _healthUpdateListener = new Mock<IEndpointHealthUpdateListener>();
            _timeCoordinator = new Mock<ITimeCoordinator>();
            _timeCoordinator.Setup(c => c.UtcNow).Returns(_utcNow);

            _sampler = new HealthSampler(settings.Object, _healthUpdateListener.Object, _timeCoordinator.Object);
            _monitor = new Mock<IHealthMonitor>();
            _monitor.Setup(m => m.Name).Returns("monitor");

            _endpoint = new MonitorableEndpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address", "token1"), _monitor.Object);
            _awaitableFactory = new AwaitableFactory();
        }

        [Fact]
        public async Task CheckHealthAsync_should_measure_monitor_execution()
        {
            var expectedTime = TimeSpan.FromMilliseconds(300);
            SetupMonitor(token => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithTimeline("health").RunAsync());
            SetupDefaultDelay();
            SetupStopWatch("watch", expectedTime);

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.Healthy, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);

            var watchEvent = _awaitableFactory.GetTimeline().Single(e => e.Tag == "watch");
            var healthEvent = _awaitableFactory.GetTimeline().Single(e => e.Tag == "health");

            Assert.True(watchEvent.Started < healthEvent.Started, "watchEvent.Started<healthEvent.Started");
            Assert.True(watchEvent.Finished > healthEvent.Finished, "watchEvent.Finished>healthEvent.Finished");
        }

        [Fact]
        public async Task CheckHealthAsync_should_measure_monitor_failures()
        {
            var expectedTime = TimeSpan.FromMilliseconds(300);
            var exception = new Exception("failure");
            SetupMonitor(token => _awaitableFactory.Throw<HealthInfo>(exception).WithTimeline("health").RunAsync());
            SetupDefaultDelay();
            SetupStopWatch("watch", expectedTime);

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);
            var expectedDetails = new Dictionary<string, string>
            {
                { "reason", exception.Message },
                { "exception", exception.ToString() }
            };
            Assert.Equal(expectedDetails, result.Details);

            var watchEvent = _awaitableFactory.GetTimeline().Single(e => e.Tag == "watch");
            var healthEvent = _awaitableFactory.GetTimeline().Single(e => e.Tag == "health");

            Assert.True(watchEvent.Started < healthEvent.Started, "watchEvent.Started<healthEvent.Started");
            Assert.True(watchEvent.Finished > healthEvent.Finished, "watchEvent.Finished>healthEvent.Finished");
        }

        [Fact]
        public async Task CheckHealthAsync_should_measure_monitor_timeouts()
        {
            var expectedTime = _shortTimeOut.Add(TimeSpan.FromMilliseconds(1));
            SetupMonitor(token => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithDelay(expectedTime, token).RunAsync());

            SetupDelayToReturnImmediatelyOn(_shortTimeOut);
            SetupStopWatch("watch", expectedTime);

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);
        }

        [Fact]
        public async Task CheckHealthAsync_should_cancel_monitor_operation_on_timeout()
        {
            var expectedTime = _shortTimeOut.Add(TimeSpan.FromMilliseconds(1));
            bool wasCancelled = false;

            SetupMonitor(async token =>
            {
                try
                {
                    await Task.Delay(expectedTime, token);
                }
                catch (TaskCanceledException)
                {
                    wasCancelled = true;
                    throw;
                }
                return new HealthInfo(HealthStatus.Healthy);
            });

            SetupDelayToReturnImmediatelyOn(_shortTimeOut);
            SetupStopWatch("watch", expectedTime);

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            Assert.True(wasCancelled, "Task should be cancelled");
        }

        [Theory]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckHealthAsync_should_use_long_timeout_for_endpoints_with_faulty_statuses(HealthStatus status)
        {
            var expectedTime = _failureTimeOut;

            SetupDefaultDelay();
            SetupDelayToReturnImmediatelyOn(expectedTime);
            SetupStopWatch("watch", TimeSpan.Zero, expectedTime);
            SetupMonitor(async token =>
            {
                if (_endpoint.Health == null)
                    return new HealthInfo(status);

                await Task.Delay(expectedTime.Add(TimeSpan.FromMilliseconds(1)), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            //First state
            await _endpoint.CheckHealth(_sampler, new CancellationToken());
            Assert.Equal(status.ToString(), _endpoint.Health.Status.ToString());

            //Timed out state
            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        public async Task CheckHealthAsync_should_use_short_timeout_for_endpoints_with_non_faulty_statuses(HealthStatus status)
        {
            var expectedTime = _shortTimeOut;

            SetupDefaultDelay();
            SetupDelayToReturnImmediatelyOn(expectedTime);
            SetupStopWatch("watch", TimeSpan.Zero, expectedTime);
            SetupMonitor(async token =>
            {
                if (_endpoint.Health == null)
                    return new HealthInfo(status);

                await Task.Delay(expectedTime.Add(TimeSpan.FromMilliseconds(1)), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            //First state
            await _endpoint.CheckHealth(_sampler, new CancellationToken());
            Assert.Equal(status.ToString(), _endpoint.Health.Status.ToString());

            //Timed out state
            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckHealthAsync_should_properly_return_all_health_statuses(HealthStatus expectedStatus)
        {
            SetupDefaultDelay();
            SetupStopWatch("watch", TimeSpan.Zero);
            var expectedDetails = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
            SetupMonitor(token => Task.FromResult(new HealthInfo(expectedStatus, expectedDetails)));

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(expectedStatus.ToString(), result.Status.ToString());
            Assert.Equal(expectedDetails, result.Details);
        }

        [Fact]
        public async Task CheckHealthAsync_should_update_endpoint_health()
        {
            SetupDefaultDelay();
            SetupStopWatch("watch", TimeSpan.Zero);
            SetupMonitor(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)));

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            _healthUpdateListener.Verify(m => m.UpdateHealth(_endpoint.Identity.Id, result));
        }

        [Fact]
        public async Task CheckHealthAsync_should_return_unhealthy_status_if_monitor_returned_healthy_after_too_long_time()
        {
            var expectedTime = _healthyResponseTimeLimit.Add(TimeSpan.FromMilliseconds(1));
            SetupMonitor(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)));
            SetupDefaultDelay();
            SetupStopWatch("watch", expectedTime);

            var result = await _sampler.CheckHealthAsync(_endpoint, new CancellationToken());
            Assert.Equal(EndpointStatus.Unhealthy, result.Status);
            Assert.Equal(expectedTime, result.ResponseTime);
            Assert.Equal(_utcNow, result.CheckTimeUtc);
        }

        [Fact]
        public async Task CheckHealthAsync_should_be_cancellable()
        {
            var maxWaitTime = _shortTimeOut.Add(TimeSpan.FromSeconds(-1));

            SetupStopWatch("watch", maxWaitTime);

            var monitorToken = CancellationToken.None;
            var delayToken = CancellationToken.None;

            SetupDelay(_shortTimeOut, async token =>
            {
                delayToken = token;
                await Task.Delay(_shortTimeOut, token);
            });

            SetupMonitor(async token =>
            {
                monitorToken = token;
                await Task.Delay(maxWaitTime, token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var cancelAfter = TimeSpan.FromMilliseconds(200);
            var src = new CancellationTokenSource(cancelAfter);
            var ex = await Assert.ThrowsAsync<AggregateException>(() => _sampler.CheckHealthAsync(_endpoint, src.Token));

            Assert.IsType<TaskCanceledException>(ex.Flatten().InnerException);
            Assert.True(monitorToken.IsCancellationRequested, "monitorToken.IsCancellationRequested");
            Assert.True(delayToken.IsCancellationRequested, "delayToken.IsCancellationRequested");
        }

        [Theory]
        [InlineData(typeof(Exception))]
        [InlineData(typeof(TaskCanceledException))]
        [InlineData(typeof(OperationCanceledException))]
        public async Task CheckHealthAsync_should_interpret_monitor_async_exceptions(Type exceptionType)
        {
            SetupDefaultDelay();
            SetupStopWatch("watch", TimeSpan.Zero);
            SetupMonitor(async token =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), token);
                throw (Exception)Activator.CreateInstance(exceptionType, "reason");
            });

            var health = await _sampler.CheckHealthAsync(_endpoint, CancellationToken.None);
            Assert.Equal(EndpointStatus.Faulty, health.Status);
            Assert.Contains($"{exceptionType.Name}: reason", health.Details["exception"]);
        }

        [Theory]
        [InlineData(typeof(Exception))]
        [InlineData(typeof(TaskCanceledException))]
        [InlineData(typeof(OperationCanceledException))]
        public async Task CheckHealthAsync_should_interpret_monitor_exceptions(Type exceptionType)
        {
            SetupDefaultDelay();
            SetupStopWatch("watch", TimeSpan.Zero);
            SetupMonitor(token =>
            {
                throw (Exception)Activator.CreateInstance(exceptionType, "reason");
            });

            var health = await _sampler.CheckHealthAsync(_endpoint, CancellationToken.None);
            Assert.Equal(EndpointStatus.Faulty, health.Status);
            Assert.Contains($"{exceptionType.Name}: reason", health.Details["exception"]);
        }
        private void SetupDefaultTimeouts(Mock<IMonitorSettings> settings)
        {
            settings.Setup(s => s.ShortTimeOut).Returns(_shortTimeOut);
            settings.Setup(s => s.HealthyResponseTimeLimit).Returns(_healthyResponseTimeLimit);
            settings.Setup(s => s.FailureTimeOut).Returns(_failureTimeOut);
        }

        private Mock<IMonitorSettings> SetupSettings()
        {
            var settings = new Mock<IMonitorSettings>();
            SetupDefaultTimeouts(settings);
            return settings;
        }

        private void SetupMonitor(Func<CancellationToken, Task<HealthInfo>> monitorAction)
        {
            _monitor
                .Setup(m => m.CheckHealthAsync(_endpoint.Identity.Address, It.IsAny<CancellationToken>()))
                .Returns((string address, CancellationToken token) => monitorAction.Invoke(token));
        }

        private void SetupStopWatch(string watchName, params TimeSpan[] expectedTimes)
        {
            var sequence = _timeCoordinator.SetupSequence(c => c.CreateStopWatch());

            foreach (var expectedTime in expectedTimes)
                sequence = sequence.Returns(_awaitableFactory.CreateStopWatch(watchName, expectedTime));
        }

        private void SetupDefaultDelay()
        {
            _timeCoordinator.Setup(c => c.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                   .Returns(async (TimeSpan t, CancellationToken c) => await Task.Delay(t, c));
        }

        private void SetupDelayToReturnImmediatelyOn(TimeSpan expectedDelay)
        {
            SetupDelay(expectedDelay, async token => await Task.Yield());
        }

        private void SetupDelay(TimeSpan expectedDelay, Func<CancellationToken, Task> action)
        {
            _timeCoordinator.Setup(c => c.Delay(expectedDelay, It.IsAny<CancellationToken>()))
                .Returns((TimeSpan t, CancellationToken c) => action(c));
        }
    }
}
