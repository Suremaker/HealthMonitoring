using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors;
using HealthMonitoring.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointTests
    {
        [Fact]
        public void Dispose_should_mark_endpoint_disposed()
        {
            var endpoint = new Endpoint(Guid.Empty, MonitorMock.Mock("monitor"), "address", "name", "group");
            Assert.False(endpoint.IsDisposed);
            endpoint.Dispose();
            Assert.True(endpoint.IsDisposed);
        }

        [Theory]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Offline)]
        public void CheckHealth_should_update_the_endpoint_with_its_health_status(HealthStatus healthStatus)
        {
            var tokenSource = new CancellationTokenSource();
            var healthInfo = new HealthInfo(healthStatus, TimeSpan.FromSeconds(1), new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } });
            var monitor = MonitorMock.GetMock("monitor");
            monitor.Setup(x => x.CheckHealthAsync("address", It.IsAny<CancellationToken>())).Returns(Task.FromResult(healthInfo));
            var statsManager = new Mock<IEndpointStatsManager>();

            var endpoint = new Endpoint(Guid.NewGuid(), monitor.Object, "address", "name", "group");

            endpoint.CheckHealth(tokenSource.Token, MonitorSettingsHelper.ConfigureDefaultSettings(), statsManager.Object).Wait();

            var health = endpoint.Health;
            Assert.NotNull(health);
            Assert.Equal(healthInfo.ResponseTime, health.ResponseTime);
            Assert.Equal(healthInfo.Status.ToString(), health.Status.ToString());
            Assert.True(DateTime.UtcNow - health.CheckTimeUtc < TimeSpan.FromMilliseconds(500), "CheckTimeUtc should be captured");
            Assert.Equal(healthInfo.Details, health.Details);

            statsManager.Verify(m => m.RecordEndpointStatistics(endpoint.Id, endpoint.Health));
        }

        [Fact]
        public void CheckHealth_should_update_the_endpoint_with_Faulty_status_if_monitor_fails()
        {
            var statsManager = new Mock<IEndpointStatsManager>();
            var tokenSource = new CancellationTokenSource();
            var monitor = MonitorMock.GetMock("monitor");
            var exception = new Exception("some error");
            monitor.Setup(x => x.CheckHealthAsync("address", It.IsAny<CancellationToken>())).Throws(exception);

            var endpoint = new Endpoint(Guid.NewGuid(), monitor.Object, "address", "name", "group");
            endpoint.CheckHealth(tokenSource.Token, MonitorSettingsHelper.ConfigureDefaultSettings(), statsManager.Object).Wait();

            var expectedDetails = new Dictionary<string, string>
            {
                { "reason", exception.Message }, 
                { "exception", exception.ToString() }
            };

            var health = endpoint.Health;
            Assert.NotNull(health);
            Assert.Equal(EndpointStatus.Faulty, health.Status);
            Assert.True(DateTime.UtcNow - health.CheckTimeUtc < TimeSpan.FromMilliseconds(500), "CheckTimeUtc should be captured");
            Assert.Equal(TimeSpan.Zero, health.ResponseTime);
            Assert.Equal(expectedDetails, health.Details);

            statsManager.Verify(m => m.RecordEndpointStatistics(endpoint.Id, endpoint.Health));
        }

        [Fact]
        public void CheckHealth_should_timeout_if_it_takes_too_long_to_process_a_request()
        {
            var statsManager = new Mock<IEndpointStatsManager>();
            var tokenSource = new CancellationTokenSource();
            var monitor = MonitorMock.GetMock("monitor");
            var endpoint = new Endpoint(Guid.NewGuid(), monitor.Object, "address", "name", "group");
            var settings = new Mock<IMonitorSettings>();

            var delay = TimeSpan.FromSeconds(5);
            var shortTimeout = TimeSpan.FromMilliseconds(50);
            settings.Setup(s => s.ShortTimeOut).Returns(shortTimeout);
            CancellationToken? monitorToken = null;
            monitor
                .Setup(m => m.CheckHealthAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((addr, token) =>
                {
                    monitorToken = token;
                })
                .Returns(ReturnWithDelay(delay, HealthStatus.Healthy));

            endpoint.CheckHealth(tokenSource.Token, settings.Object, statsManager.Object).Wait();
            Assert.NotNull(monitorToken);
            Assert.True(monitorToken.Value.IsCancellationRequested);

            Assert.Equal(EndpointStatus.TimedOut, endpoint.Health.Status);
            statsManager.Verify(m => m.RecordEndpointStatistics(endpoint.Id, endpoint.Health));
        }

        [Fact]
        public void CheckHealth_should_return_Unhealthy_if_request_takes_long_time()
        {
            var statsManager = new Mock<IEndpointStatsManager>();
            var tokenSource = new CancellationTokenSource();
            var monitor = new DelayingMonitor(TimeSpan.FromMilliseconds(100));
            var endpoint = new Endpoint(Guid.NewGuid(), monitor, "address", "name", "group");
            var settings = new Mock<IMonitorSettings>();

            settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromSeconds(1));
            settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromMilliseconds(50));

            endpoint.CheckHealth(tokenSource.Token, settings.Object, statsManager.Object).Wait();

            Assert.Equal(EndpointStatus.Unhealthy, endpoint.Health.Status);
            statsManager.Verify(m => m.RecordEndpointStatistics(endpoint.Id, endpoint.Health));
        }

        [Fact]
        public void CheckHealth_should_return_Faulty_if_request_takes_too_long_time_and_endpoint_is_already_unhealthy()
        {
            var statsManager = new Mock<IEndpointStatsManager>();
            var tokenSource = new CancellationTokenSource();
            var monitor = new DelayingMonitor(TimeSpan.FromSeconds(10));
            var endpoint = new Endpoint(Guid.NewGuid(), monitor, "address", "name", "group");
            var settings = new Mock<IMonitorSettings>();

            settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromMilliseconds(50));
            settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromMilliseconds(25));
            settings.Setup(s => s.FailureTimeOut).Returns(TimeSpan.FromMilliseconds(75));

            endpoint.CheckHealth(tokenSource.Token, settings.Object, statsManager.Object).Wait();
            Assert.Equal(EndpointStatus.TimedOut, endpoint.Health.Status);

            endpoint.CheckHealth(tokenSource.Token, settings.Object, statsManager.Object).Wait();
            Assert.Equal(EndpointStatus.Faulty, endpoint.Health.Status);
            statsManager.Verify(m => m.RecordEndpointStatistics(endpoint.Id, It.IsAny<EndpointHealth>()), Times.Exactly(2));
        }

        private async Task<HealthInfo> ReturnWithDelay(TimeSpan delay, HealthStatus status)
        {
            await Task.Delay(delay);
            return new HealthInfo(status, delay);
        }
    }
}