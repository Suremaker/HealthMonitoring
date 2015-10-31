using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
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

        [Fact]
        public void CheckHealth_should_update_the_endpoint_with_its_health_status()
        {
            var endpoint = new Endpoint(Guid.Empty, MonitorMock.Mock("monitor"), "address", "name", "group");
            var sampler = new Mock<IHealthSampler>();
            var token = new CancellationToken();
            var result = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            sampler.Setup(s => s.CheckHealth(endpoint, token)).Returns(Task.FromResult(result));
            endpoint.CheckHealth(sampler.Object, token).Wait();
            Assert.Same(result, endpoint.Health);
        }
    }
}