using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class HealthSamplerTests
    {
        private readonly HealthSampler _sampler;
        private readonly Mock<IMonitorSettings> _settings;
        private readonly Mock<IEndpointHealthUpdateListener> _healthUpdateListener;
        private readonly MockableMonitor _monitor;
        private readonly MonitorableEndpoint _endpoint;

        public HealthSamplerTests()
        {
            _settings = new Mock<IMonitorSettings>();
            _healthUpdateListener = new Mock<IEndpointHealthUpdateListener>();
            _sampler = new HealthSampler(_settings.Object, _healthUpdateListener.Object);
            _monitor = new MockableMonitor();
            _endpoint = new MonitorableEndpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), _monitor);
        }

        [Fact]
        public void CheckHealthAsync_should_measure_monitor_execution()
        {
            SetupDefaultTimeouts();

            var expectedTime = TimeSpan.FromMilliseconds(600);
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Delay(expectedTime, token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Healthy, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));
        }

        [Fact]
        public void CheckHealthAsync_should_measure_monitor_failures()
        {
            SetupDefaultTimeouts();

            var expectedTime = TimeSpan.FromMilliseconds(600);
            var exception = new Exception("failure");
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Delay(expectedTime, token);
                throw exception;
            });

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));

            var expectedDetails = new Dictionary<string, string>
            {
                { "reason", exception.Message },
                { "exception", exception.ToString() }
            };
            Assert.Equal(expectedDetails, result.Details);
        }

        [Fact]
        public void CheckHealthAsync_should_measure_monitor_timeouts()
        {
            var expectedTime = TimeSpan.FromMilliseconds(600);
            _settings.Setup(s => s.ShortTimeOut).Returns(expectedTime);

            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));
        }

        [Fact]
        public void CheckHealthAsync_should_cancel_monitor_operation_on_timeout()
        {
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromMilliseconds(300));
            bool wasCancelled = false;
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                }
                catch (TaskCanceledException)
                {
                    wasCancelled = true;
                    throw;
                }
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            Assert.True(wasCancelled, "Task should be cancelled");
        }

        [Theory]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public void CheckHealthAsync_should_use_long_timeout_for_endpoints_with_faulty_statuses(HealthStatus status)
        {
            var expectedTime = TimeSpan.FromMilliseconds(600);
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromMilliseconds(30));
            _settings.Setup(s => s.FailureTimeOut).Returns(expectedTime);

            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                if (_endpoint.Health == null)
                    return new HealthInfo(status);
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            //First state
            _endpoint.CheckHealth(_sampler, new CancellationToken()).Wait();
            Assert.Equal(status.ToString(), _endpoint.Health.Status.ToString());

            //Timed out state
            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        public void CheckHealthAsync_should_use_short_timeout_for_endpoints_with_non_faulty_statuses(HealthStatus status)
        {
            var expectedTime = TimeSpan.FromMilliseconds(600);
            _settings.Setup(s => s.ShortTimeOut).Returns(expectedTime);
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.FailureTimeOut).Returns(TimeSpan.FromSeconds(1));

            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                if (_endpoint.Health == null)
                    return new HealthInfo(status);
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            //First state
            _endpoint.CheckHealth(_sampler, new CancellationToken()).Wait();
            Assert.Equal(status.ToString(), _endpoint.Health.Status.ToString());

            //Timed out state
            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public void CheckHealthAsync_should_properly_return_all_health_statuses(HealthStatus expectedStatus)
        {
            SetupDefaultTimeouts();
            var expectedDetails = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
            _monitor.ExpectFor(_endpoint.Identity.Address, token => Task.FromResult(new HealthInfo(expectedStatus, expectedDetails)));

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(expectedStatus.ToString(), result.Status.ToString());
            Assert.Equal(expectedDetails, result.Details);
        }

        [Fact]
        public void CheckHealthAsync_should_update_endpoint_health()
        {
            SetupDefaultTimeouts();
            _monitor.ExpectFor(_endpoint.Identity.Address, token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)));

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            _healthUpdateListener.Verify(m => m.UpdateHealth(_endpoint.Identity.Id, result));
        }

        [Fact]
        public void CheckHealthAsync_should_return_unhealthy_status_if_monitor_returned_healthy_after_too_long_time()
        {
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromMilliseconds(200));

            var expectedTime = TimeSpan.FromMilliseconds(500);
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Delay(expectedTime, token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealthAsync(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Unhealthy, result.Status);
            AssertResponseTime(expectedTime, result.ResponseTime);
            AssertCheckTime(result, TimeSpan.FromMilliseconds(200));
        }

        [Fact]
        public async Task CheckHealthAsync_should_be_cancellable()
        {
            SetupDefaultTimeouts();
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Yield();
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var cancelAfter = TimeSpan.FromMilliseconds(400);
            var src = new CancellationTokenSource(cancelAfter);
            var watch = Stopwatch.StartNew();
            await Assert.ThrowsAsync<AggregateException>(() => _sampler.CheckHealthAsync(_endpoint, src.Token));

            AssertResponseTime(cancelAfter, watch.Elapsed, TimeSpan.FromMilliseconds(200));
        }

        [Fact]
        public async Task CheckHealthAsync_should_interpret_monitor_async_exceptions()
        {
            SetupDefaultTimeouts();
            _monitor.ExpectFor(_endpoint.Identity.Address, async token =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), token);
                throw new InvalidOperationException("reason");
            });

            var health = await _sampler.CheckHealthAsync(_endpoint, CancellationToken.None);
            Assert.Equal(EndpointStatus.Faulty, health.Status);
            Assert.Contains("InvalidOperationException: reason", health.Details["exception"]);
        }

        [Fact]
        public async Task CheckHealthAsync_should_interpret_monitor_exceptions()
        {
            SetupDefaultTimeouts();
            _monitor.ExpectFor(_endpoint.Identity.Address, token =>
            {
                throw new InvalidOperationException("reason");
            });

            var health = await _sampler.CheckHealthAsync(_endpoint, CancellationToken.None);
            Assert.Equal(EndpointStatus.Faulty, health.Status);
            Assert.Contains("InvalidOperationException: reason", health.Details["exception"]);
        }

        private void AssertResponseTime(TimeSpan expectedTime, TimeSpan actualTime)
        {
            AssertResponseTime(expectedTime, actualTime, TimeSpan.FromMilliseconds(200));
        }

        private static void AssertResponseTime(TimeSpan expectedTime, TimeSpan actualTime, TimeSpan delta)
        {
            var difference = actualTime - expectedTime;
            Assert.True(difference.Duration() < delta,
                $"Expected ResponseTime being {expectedTime} ~ {delta.TotalMilliseconds}ms, got: {actualTime}");
        }

        private static void AssertCheckTime(EndpointHealth result, TimeSpan delta)
        {
            var now = DateTime.UtcNow;
            var dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
            var difference = (now - result.CheckTimeUtc) - result.ResponseTime;
            Assert.True(difference.Duration() < delta,
                $"Expected CheckTimeUtc being {(now - result.ResponseTime).ToString(dateFormat)} ~ {delta.TotalMilliseconds}ms, got: {result.CheckTimeUtc.ToString(dateFormat)}");
        }

        private void SetupDefaultTimeouts()
        {
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(5));
        }
    }
}
