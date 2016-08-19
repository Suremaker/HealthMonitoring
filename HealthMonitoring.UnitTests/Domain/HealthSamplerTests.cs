using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class HealthSamplerTests
    {
        private readonly HealthSampler _sampler;
        private readonly Mock<IMonitorSettings> _settings;
        private readonly Mock<IEndpointStatsManager> _statsManager;
        private readonly Mock<IHealthMonitor> _monitor;
        private readonly Endpoint _endpoint;

        public HealthSamplerTests()
        {
            _settings = new Mock<IMonitorSettings>();
            _statsManager = new Mock<IEndpointStatsManager>();
            _sampler = new HealthSampler(_settings.Object, _statsManager.Object);
            _monitor = new Mock<IHealthMonitor>();
            _endpoint = new Endpoint(Guid.NewGuid(), _monitor.Object, "address", "name", "group", new[] { "t1", "t2" });
        }

        [Fact]
        public void CheckHealth_should_measure_monitor_execution()
        {
            SetupDefaultTimeouts();

            var expectedTime = TimeSpan.FromMilliseconds(300);
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(async () =>
            {
                await Task.Delay(expectedTime);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Healthy, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);
        }

        [Fact]
        public void CheckHealth_should_measure_monitor_failures()
        {
            SetupDefaultTimeouts();

            var expectedTime = TimeSpan.FromMilliseconds(300);
            var exception = new Exception("failure");
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(async () =>
            {
                await Task.Delay(expectedTime);
                throw exception;
            });

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);

            var expectedDetails = new Dictionary<string, string>
            {
                { "reason", exception.Message }, 
                { "exception", exception.ToString() }
            };
            Assert.Equal(expectedDetails, result.Details);
        }

        [Fact]
        public void CheckHealth_should_measure_monitor_timeouts()
        {
            var expectedTime = TimeSpan.FromMilliseconds(300);
            _settings.Setup(s => s.ShortTimeOut).Returns(expectedTime);

            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(async (string address, CancellationToken token) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);
        }

        [Fact]
        public void CheckHealth_should_cancel_monitor_operation_on_timeout()
        {
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromMilliseconds(300));
            bool wasCancelled = false;
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(async (string address, CancellationToken token) =>
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

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            Assert.True(wasCancelled, "Task should be cancelled");
        }

        [Theory]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public void CheckHealth_should_use_long_timeout_for_endpoints_with_faulty_statuses(HealthStatus status)
        {
            var expectedTime = TimeSpan.FromMilliseconds(300);
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromMilliseconds(30));
            _settings.Setup(s => s.FailureTimeOut).Returns(expectedTime);

            _monitor
                .Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>()))
                .Returns(async (string address, CancellationToken token) =>
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
            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Faulty, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        public void CheckHealth_should_use_short_timeout_for_endpoints_with_non_faulty_statuses(HealthStatus status)
        {
            var expectedTime = TimeSpan.FromMilliseconds(300);
            _settings.Setup(s => s.ShortTimeOut).Returns(expectedTime);
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.FailureTimeOut).Returns(TimeSpan.FromSeconds(1));

            _monitor
                .Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>()))
                .Returns(async (string address, CancellationToken token) =>
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
            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.TimedOut, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.NotExists)]
        [InlineData(HealthStatus.Offline)]
        [InlineData(HealthStatus.TimedOut)]
        [InlineData(HealthStatus.Unhealthy)]
        public void CheckHealth_should_properly_return_all_health_statuses(HealthStatus expectedStatus)
        {
            SetupDefaultTimeouts();
            var expectedDetails = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new HealthInfo(expectedStatus, expectedDetails)));

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(expectedStatus.ToString(), result.Status.ToString());
            Assert.Equal(expectedDetails, result.Details);
        }

        [Fact]
        public void CheckHealth_should_update_endpoint_statistics()
        {
            SetupDefaultTimeouts();
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new HealthInfo(HealthStatus.Healthy)));

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            _statsManager.Verify(m => m.RecordEndpointStatistics(_endpoint.Id, result));
        }

        [Fact]
        public void CheckHealth_should_return_unhealthy_status_if_monitor_returned_healthy_after_too_long_time()
        {
            _settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromSeconds(5));
            _settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromMilliseconds(200));

            var expectedTime = TimeSpan.FromMilliseconds(300);
            _monitor.Setup(m => m.CheckHealthAsync(_endpoint.Address, It.IsAny<CancellationToken>())).Returns(async () =>
            {
                await Task.Delay(expectedTime);
                return new HealthInfo(HealthStatus.Healthy);
            });

            var result = _sampler.CheckHealth(_endpoint, new CancellationToken()).Result;
            Assert.Equal(EndpointStatus.Unhealthy, result.Status);
            AssertResponseTime(result, expectedTime);
            AssertCheckTime(result);
        }

        private void AssertResponseTime(EndpointHealth result, TimeSpan expectedTime)
        {
            var delta = TimeSpan.FromMilliseconds(100);
            var difference = result.ResponseTime - expectedTime;
            Assert.True(difference.Duration() < delta,
                $"Expected ResponseTime being {expectedTime} ~ {delta.TotalMilliseconds}ms, got: {result.ResponseTime}");
        }

        private static void AssertCheckTime(EndpointHealth result)
        {
            var now = DateTime.UtcNow;
            var delta = TimeSpan.FromMilliseconds(100);
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
