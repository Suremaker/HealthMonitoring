using System;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointTests
    {
        [Fact]
        public async Task UpdateHealth_should_update_health_and_last_modified_time()
        {
            var endpoint = new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group"));
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            endpoint.UpdateHealth(health);
            Assert.Same(health, endpoint.Health);

            AssertTime(endpoint, DateTimeOffset.UtcNow, endpoint.LastModifiedTime, TimeSpan.FromMilliseconds(50));
        }

        [Fact]
        public async Task UpdateHealth_should_update_health_and_last_modified_time_if_newer_health_is_provided()
        {
            var endpoint = new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group"));
            var oldHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            endpoint.UpdateHealth(oldHealth);
            Assert.Same(oldHealth, endpoint.Health);


            await Task.Delay(TimeSpan.FromMilliseconds(200));

            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            endpoint.UpdateHealth(health);
            Assert.Same(health, endpoint.Health);

            AssertTime(endpoint, DateTimeOffset.UtcNow, endpoint.LastModifiedTime, TimeSpan.FromMilliseconds(50));
        }

        [Fact]
        public void UpdateHealth_should_not_update_health_if_already_have_more_recent_result()
        {
            var endpoint = new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group"));
            var newHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            var oldHealth = new EndpointHealth(DateTime.UtcNow.AddSeconds(-1), TimeSpan.Zero, EndpointStatus.Healthy);

            endpoint.UpdateHealth(newHealth);
            endpoint.UpdateHealth(oldHealth);
            Assert.Same(newHealth, endpoint.Health);
        }

        [Fact]
        public async Task UpdateMetadata_should_update_metadata_and_last_modified_time()
        {
            var endpoint = new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group"));

            await Task.Delay(TimeSpan.FromMilliseconds(200));
            endpoint.UpdateMetadata("new group", "new name");

            Assert.Equal("new group", endpoint.Metadata.Group);
            Assert.Equal("new name", endpoint.Metadata.Name);

            AssertTime(endpoint, DateTimeOffset.UtcNow, endpoint.LastModifiedTime, TimeSpan.FromMilliseconds(50));
        }

        [Fact]
        public async Task Dispose_should_remove_health_information_and_update_last_modified_time_as_well_as_IsDisposed_property()
        {
            var endpoint = new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group"));
            endpoint.UpdateHealth(new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy));

            await Task.Delay(TimeSpan.FromMilliseconds(200));
            endpoint.Dispose();

            Assert.True(endpoint.IsDisposed);
            Assert.Null(endpoint.Health);

            AssertTime(endpoint, DateTimeOffset.UtcNow, endpoint.LastModifiedTime, TimeSpan.FromMilliseconds(50));
        }

        private static void AssertTime(Endpoint endpoint, DateTimeOffset expected, DateTimeOffset actual, TimeSpan delta)
        {
            Assert.True((expected - actual).Duration() < delta, string.Format("Expected: {0} +- {1}, got: {2}", expected, actual, delta));
        }
    }
}