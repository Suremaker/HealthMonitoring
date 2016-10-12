using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class MonitorableEndpointTests
    {
        [Fact]
        public void Dispose_should_mark_endpoint_disposed()
        {
            var endpoint = new MonitorableEndpoint(new EndpointIdentity(Guid.Empty, "monitor", "address", "token1"), MonitorMock.Mock("monitor"));
            Assert.False(endpoint.IsDisposed);
            endpoint.Dispose();
            Assert.True(endpoint.IsDisposed);
        }

        [Fact]
        public async Task CheckHealth_should_update_the_endpoint_with_its_health_status()
        {
            var endpoint = new MonitorableEndpoint(new EndpointIdentity(Guid.Empty, "monitor", "address", "token1"), MonitorMock.Mock("monitor"));
            var sampler = new Mock<IHealthSampler>();
            var token = new CancellationToken();
            var result = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            sampler.Setup(s => s.CheckHealthAsync(endpoint, token)).Returns(Task.FromResult(result));
            await endpoint.CheckHealth(sampler.Object, token);
            Assert.Same(result, endpoint.Health);
        }
    }
}