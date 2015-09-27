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

            var endpoint = new Endpoint(Guid.NewGuid(), monitor.Object, "address", "name", "group");

            endpoint.CheckHealth(tokenSource.Token, MonitorSettingsHelper.ConfigureDefaultSettings()).Wait();

            var health = endpoint.Health;
            Assert.NotNull(health);
            Assert.Equal(healthInfo.ResponseTime, health.ResponseTime);
            Assert.Equal(healthInfo.Status.ToString(), health.Status.ToString());
            Assert.True(DateTime.UtcNow - health.CheckTimeUtc < TimeSpan.FromMilliseconds(500), "CheckTimeUtc should be captured");
            Assert.Equal(healthInfo.Details, health.Details);
        }

        [Fact]
        public void CheckHealth_should_update_the_endpoint_with_Faulty_status_if_monitor_fails()
        {
            var tokenSource = new CancellationTokenSource();
            var monitor = MonitorMock.GetMock("monitor");
            var exception = new Exception("some error");
            monitor.Setup(x => x.CheckHealthAsync("address", It.IsAny<CancellationToken>())).Throws(exception);

            var endpoint = new Endpoint(Guid.NewGuid(), monitor.Object, "address", "name", "group");
            endpoint.CheckHealth(tokenSource.Token, MonitorSettingsHelper.ConfigureDefaultSettings()).Wait();

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
        }
    }
}