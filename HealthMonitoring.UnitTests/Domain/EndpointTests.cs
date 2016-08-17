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
            var endpoint = new Endpoint(Guid.Empty, MonitorMock.Mock("monitor"), "address", "name", "group", new[] { "t1", "t2" });
            Assert.False(endpoint.IsDisposed);
            endpoint.Dispose();
            Assert.True(endpoint.IsDisposed);
        }

        [Fact]
        public void CheckHealth_should_update_the_endpoint_with_its_health_status()
        {
            var endpoint = new Endpoint(Guid.Empty, MonitorMock.Mock("monitor"), "address", "name", "group", new[] { "t1", "t2" });
            var lastModifiedTime = endpoint.LastModifiedTime;
            var sampler = new Mock<IHealthSampler>();
            var token = new CancellationToken();
            var result = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            Thread.Sleep(100);
            sampler.Setup(s => s.CheckHealth(endpoint, token)).Returns(Task.FromResult(result));
            endpoint.CheckHealth(sampler.Object, token).Wait();
            Assert.Same(result, endpoint.Health);
            Assert.True(endpoint.LastModifiedTime > lastModifiedTime, "LastModifiedTime should be updated");
        }

        [Fact]
        public async void CheckHealth_should_not_update_disposed_endpoint()
        {
            var endpoint = new Endpoint(Guid.Empty, MonitorMock.Mock("monitor"), "address", "name", "group", new[] { "t1", "t2" });

            var sampler = new Mock<IHealthSampler>();
            var token = new CancellationToken();
            var result = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);

            sampler.Setup(s => s.CheckHealth(endpoint, token)).Returns(async () =>
            {
                await Task.Yield();
                return result;
            });

            var task = endpoint.CheckHealth(sampler.Object, token);
            Thread.Sleep(100);
            endpoint.Dispose();
            var lastModifiedTime = endpoint.LastModifiedTime;
            Thread.Sleep(100);
            await task;

            Assert.True(endpoint.IsDisposed);
            Assert.Null(endpoint.Health);
            Assert.Equal(lastModifiedTime, endpoint.LastModifiedTime);
        }
    }
}