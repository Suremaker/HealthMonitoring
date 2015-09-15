using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Protocols;
using HealthMonitoring.UnitTests.Helpers;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointTests
    {
        [Fact]
        public void Dispose_should_mark_endpoint_disposed()
        {
            var endpoint = new Endpoint(Guid.Empty, ProtocolMock.Mock("proto"), "address", "name", "group");
            Assert.False(endpoint.IsDisposed);
            endpoint.Dispose();
            Assert.True(endpoint.IsDisposed);
        }

        [Theory]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Inactive)]
        public void CheckHealth_should_update_the_endpoint_with_its_health_status(HealthStatus healthStatus)
        {
            var tokenSource = new CancellationTokenSource();
            var healthInfo = new HealthInfo(healthStatus, TimeSpan.FromSeconds(1), new Dictionary<string, string> { { "property1", "value1" }, { "property2", "value2" } });
            var protocol = ProtocolMock.GetMock("proto");
            protocol.Setup(x => x.CheckHealthAsync("address", tokenSource.Token)).Returns(Task.FromResult(healthInfo));

            var endpoint = new Endpoint(Guid.NewGuid(), protocol.Object, "address", "name", "group");

            endpoint.CheckHealth(tokenSource.Token).Wait();

            var health = endpoint.Health;
            Assert.NotNull(health);
            Assert.Equal(healthInfo.ResponseTime, health.ResponseTime);
            Assert.Equal(healthInfo.Status.ToString(), health.Status.ToString());
            Assert.True(DateTime.UtcNow - health.CheckTimeUtc < TimeSpan.FromMilliseconds(500), "CheckTimeUtc should be captured");
            Assert.Equal(healthInfo.Details, health.Details);
        }

        [Fact]
        public void CheckHealth_should_update_the_endpoint_with_Faulty_status_if_protocol_fails()
        {
            var tokenSource = new CancellationTokenSource();
            var protocol = ProtocolMock.GetMock("proto");
            var exception = new Exception("some error");
            protocol.Setup(x => x.CheckHealthAsync("address", tokenSource.Token)).Throws(exception);

            var endpoint = new Endpoint(Guid.NewGuid(), protocol.Object, "address", "name", "group");
            endpoint.CheckHealth(tokenSource.Token).Wait();

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