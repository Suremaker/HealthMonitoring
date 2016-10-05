using System;
using System.Linq;
using System.Threading;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointRegistryTests
    {
        private readonly EndpointRegistry _registry;
        private readonly Mock<IHealthMonitorTypeRegistry> _healthMonitorTypeRegistry;
        private readonly Mock<IEndpointConfigurationRepository> _configurationStore;
        private readonly Mock<IEndpointStatsRepository> _statsRepository;
        private readonly Mock<IEndpointMetricsForwarderCoordinator> _forwarderCoordinator;
        private readonly Mock<IEndpointStatsManager> _statsManager;
        private readonly Mock<ITimeCoordinator> _timeCoordinator = new Mock<ITimeCoordinator>();

        public EndpointRegistryTests()
        {
            _healthMonitorTypeRegistry = new Mock<IHealthMonitorTypeRegistry>();
            _configurationStore = new Mock<IEndpointConfigurationRepository>();
            _statsRepository = new Mock<IEndpointStatsRepository>();
            _forwarderCoordinator = new Mock<IEndpointMetricsForwarderCoordinator>();
            _registry = new EndpointRegistry(_healthMonitorTypeRegistry.Object, _configurationStore.Object, _statsRepository.Object, _forwarderCoordinator.Object, _statsManager.Object, _timeCoordinator.Object);
        }

        [Fact]
        public void EndpointRegistry_should_load_endpoints_from_repository()
        {
            var endpoint = new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), "monitor", "address"), new EndpointMetadata("name", "group", new[] { "t1", "t2" }));
            _configurationStore.Setup(s => s.LoadEndpoints()).Returns(new[] { endpoint });

            var registry = new EndpointRegistry(_healthMonitorTypeRegistry.Object, _configurationStore.Object, _statsRepository.Object, _forwarderCoordinator.Object, _statsManager.Object, _timeCoordinator.Object);

            Assert.Same(endpoint, registry.GetById(endpoint.Identity.Id));
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_which_should_be_retrievable_later_by_GetById()
        {
            SetupMonitors("monitor");

            var expectedLastModifiedTime = DateTime.UtcNow;
            _timeCoordinator.Setup(c => c.UtcNow).Returns(expectedLastModifiedTime);

            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1" });
            Assert.NotEqual(Guid.Empty, id);

            var endpoint = _registry.GetById(id);

            Assert.NotNull(endpoint);
            Assert.Equal("monitor", endpoint.Identity.MonitorType);
            Assert.Equal("address", endpoint.Identity.Address);
            Assert.Equal("name", endpoint.Metadata.Name);
            Assert.Equal("group", endpoint.Metadata.Group);
            Assert.Equal(id, endpoint.Identity.Id);
            Assert.Equal("t1", endpoint.Metadata.Tags[0]);
            Assert.Equal(expectedLastModifiedTime, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void RegisterOrUpdate_shouldnt_update_tags_if_null_passed()
        {
            SetupMonitors("monitor");

            var endpoint = _registry.GetById(_registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1" }));

            Assert.NotNull(endpoint);
            Assert.Equal("t1", endpoint.Metadata.Tags[0]);

            endpoint = _registry.GetById(_registry.RegisterOrUpdate("monitor", "address", "group", "name", null));

            Assert.NotNull(endpoint);
            Assert.Equal("t1", endpoint.Metadata.Tags[0]);
        }

        [Fact]
        public void RegisterOrUpdate_should_not_update_existing_tags_if_null_passed()
        {
            SetupMonitors("monitor");

            var endpoint = _registry.GetById(_registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1", "t2" }));

            Assert.NotNull(endpoint);

            endpoint = _registry.GetById(_registry.RegisterOrUpdate("monitor", "address", "group", "name", null));

            Assert.Equal(new[] { "t1", "t2" }, endpoint.Metadata.Tags);
        }

        [Fact]
        public void RegisterOrUpdate_should_save_new_endpoint_to_repository_when_it_is_created_or_updated()
        {
            SetupMonitors("monitor");

            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1", "t2" });

            _configurationStore.Verify(s => s.SaveEndpoint(It.Is<Endpoint>(e => e.Identity.Id == id)));

            var newName = "name1";
            _registry.RegisterOrUpdate("monitor", "address", "group", newName, new[] { "t1", "t2" });
            _configurationStore.Verify(s => s.SaveEndpoint(It.Is<Endpoint>(e => e.Identity.Id == id && e.Metadata.Name == newName)));
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_if_monitor_and_address_pair_is_different()
        {
            SetupMonitors("monitor", "monitor1");

            var id1 = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1", "t2" });
            var id2 = _registry.RegisterOrUpdate("monitor1", "address", "group", "name", new[] { "t1", "t2" });
            var id3 = _registry.RegisterOrUpdate("monitor", "address1", "group", "name", new[] { "t1", "t2" });

            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id1, id3);
            Assert.NotEqual(id2, id3);
        }

        [Fact]
        public void RegisterOrUpdate_should_update_existing_endpoint_and_return_same_id()
        {
            SetupMonitors("monitor");

            var updateTime1 = DateTime.UtcNow;
            var updateTime2 = DateTime.UtcNow.AddMinutes(1);
            _timeCoordinator.Setup(c => c.UtcNow)
                .Returns(updateTime1);

            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1", "t2" });
            Assert.Equal(updateTime1, _registry.GetById(id).LastModifiedTimeUtc);

            _timeCoordinator.Setup(c => c.UtcNow)
                .Returns(updateTime2);

            var id2 = _registry.RegisterOrUpdate("monitor", "ADDRESS", "group2", "name2", new[] { "t1", "t2" });

            Assert.Equal(id, id2);

            var endpoint = _registry.GetById(id);
            Assert.NotNull(endpoint);
            Assert.Equal("monitor", endpoint.Identity.MonitorType);
            Assert.Equal("address", endpoint.Identity.Address);
            Assert.Equal("name2", endpoint.Metadata.Name);
            Assert.Equal("group2", endpoint.Metadata.Group);
            Assert.Equal(updateTime2, endpoint.LastModifiedTimeUtc);
        }

        [Fact]
        public void TryUnregister_should_remove_endpoint_and_dispose_it()
        {
            SetupMonitors("monitor");
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new[] { "t1", "t2" });
            var endpoint = _registry.GetById(id);
            Assert.True(_registry.TryUnregisterById(id), "Endpoint should be unregistered");
            Assert.True(endpoint.IsDisposed, "Endpoint should be disposed");
            Assert.Null(_registry.GetById(id));

            _configurationStore.Verify(s => s.DeleteEndpoint(id));
            _statsRepository.Verify(s => s.DeleteStatistics(id));
        }

        [Fact]
        public void TryUnregister_should_return_false_if_endpoint_is_not_registered()
        {
            Assert.False(_registry.TryUnregisterById(Guid.NewGuid()));
        }

        [Fact]
        public void GetById_should_return_null_for_unknown_id()
        {
            Assert.Null(_registry.GetById(Guid.NewGuid()));
        }

        [Fact]
        public void RegisterOrUpdate_should_throw_UnsupportedMonitorException_if_monitor_is_not_recognized()
        {
            _healthMonitorTypeRegistry.Setup(r => r.GetMonitorTypes()).Returns(new string[0]);
            var exception = Assert.Throws<UnsupportedMonitorException>(() => _registry.RegisterOrUpdate("monitor", "a", "b", "c", new[] { "t1", "t2" }));
            Assert.Equal("Unsupported monitor: monitor", exception.Message);
        }

        [Fact]
        public void Endpoints_should_return_all_endpoints()
        {
            SetupMonitors("monitor");
            var id1 = _registry.RegisterOrUpdate("monitor", "address", "group", "name", null);
            var id2 = _registry.RegisterOrUpdate("monitor", "address2", "group", "name", null);

            Assert.Equal(new[] { id1, id2 }.OrderBy(i => i).ToArray(), _registry.Endpoints.Select(e => e.Identity.Id).OrderBy(i => i).ToArray());
        }

        [Fact]
        public void UpdateHealth_should_update_health_and_save_it_in_repository()
        {
            SetupMonitors("monitor");
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", null);
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            _registry.UpdateHealth(id, health);
            _statsManager.Verify(r => r.RecordEndpointStatistics(id, health));
            Assert.Same(_registry.GetById(id).Health, health);
        }

        [Fact]
        public void UpdateHealth_should_ignore_unknown_edpoints()
        {
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Healthy);
            var id = Guid.NewGuid();
            _registry.UpdateHealth(id, health);
            _statsRepository.Verify(r => r.InsertEndpointStatistics(id, health), Times.Never);
        }

        [Fact]
        public void RegisterOrUpdate_should_trigger_EndpointAdded_event()
        {
            SetupMonitors("monitor");
            Endpoint captured = null;

            _registry.EndpointAdded += e => { captured = e; };
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name", new string[0]);
            Assert.NotNull(captured);
            Assert.Same(captured, _registry.GetById(id));
        }

        [Fact]
        public void RegisterOrUpdate_should_not_trigger_EndpointAdded_event_if_endpoint_already_exists()
        {
            SetupMonitors("monitor");
            _registry.RegisterOrUpdate("monitor", "address", "group", "name", new string[0]);

            Endpoint captured = null;
            _registry.EndpointAdded += e => { captured = e; };
            _registry.RegisterOrUpdate("monitor", "address", "group", "name", new string[0]);

            Assert.Null(captured);
        }

        private void SetupMonitors(params string[] monitorTypes)
        {
            _healthMonitorTypeRegistry
                .Setup(r => r.GetMonitorTypes())
                .Returns(monitorTypes);
        }
    }
}