using System;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointTests
    {
        private readonly Mock<ITimeCoordinator> _timeCoordinator = new Mock<ITimeCoordinator>();

        [Fact]
        public void Endpoint_should_be_created_with_last_modified_time_set()
        {
            var expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", null));


            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void UpdateHealth_should_update_health_and_last_modified_time()
        {
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", null));
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);

            var expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);

            endpoint.UpdateHealth(health);
            Assert.Same(health, endpoint.Health);

            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void UpdateHealth_should_update_health_and_last_modified_time_if_newer_health_is_provided()
        {
            var expectedLastModifiedTime1 = DateTime.UtcNow;
            var expectedLastModifiedTime2 = DateTime.UtcNow.AddMinutes(1);

            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", null));
            var oldHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);

            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime1);
            endpoint.UpdateHealth(oldHealth);
            Assert.Same(oldHealth, endpoint.Health);
            Assert.Equal(expectedLastModifiedTime1, endpoint.LastModifiedTimeUtc);


            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime2);
            endpoint.UpdateHealth(health);
            Assert.Same(health, endpoint.Health);
            Assert.Equal(expectedLastModifiedTime2, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void UpdateHealth_should_not_update_health_if_already_have_more_recent_result()
        {
            var expectedLastModifiedTime1 = DateTime.UtcNow;
            var expectedLastModifiedTime2 = DateTime.UtcNow.AddMinutes(1);

            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", null));
            var newHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            var oldHealth = new EndpointHealth(DateTime.UtcNow.AddSeconds(-1), TimeSpan.Zero, EndpointStatus.Healthy);

            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime1);
            endpoint.UpdateHealth(newHealth);

            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime2);
            endpoint.UpdateHealth(oldHealth);

            Assert.Same(newHealth, endpoint.Health);
            Assert.Equal(expectedLastModifiedTime1, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void UpdateMetadata_should_update_metadata_and_last_modified_time()
        {
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", new[] { "t1" }));

            var expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);

            endpoint.UpdateMetadata("new group", "new name", new[] { "t1", "t2" });

            Assert.Equal("new group", endpoint.Metadata.Group);
            Assert.Equal("new name", endpoint.Metadata.Name);
            Assert.Equal(new[] { "t1", "t2" }, endpoint.Metadata.Tags);

            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void UpdateMetadata_should_not_update_tags_if_null_provided()
        {
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", new[] { "t1" }));

            var expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);
            endpoint.UpdateMetadata("new group", "new name", null);

            Assert.Equal("new group", endpoint.Metadata.Group);
            Assert.Equal("new name", endpoint.Metadata.Name);
            Assert.Equal(new[] { "t1" }, endpoint.Metadata.Tags);
            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void Dispose_should_remove_health_information_and_update_last_modified_time_as_well_as_IsDisposed_property()
        {
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", null));
            endpoint.UpdateHealth(new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy));

            DateTime expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);

            endpoint.Dispose();

            Assert.True(endpoint.IsDisposed);
            Assert.Null(endpoint.Health);
            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }
    }
}