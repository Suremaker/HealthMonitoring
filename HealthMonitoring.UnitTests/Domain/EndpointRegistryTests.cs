using System;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors;
using HealthMonitoring.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointRegistryTests
    {
        private readonly EndpointRegistry _registry;
        private readonly Mock<IHealthMonitorRegistry> _monitorRegistry;

        public EndpointRegistryTests()
        {
            _monitorRegistry = new Mock<IHealthMonitorRegistry>();
            _registry = new EndpointRegistry(_monitorRegistry.Object);
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_and_emit_NewEndpointAdded_event()
        {
            MockMonitor("monitor");

            Endpoint endpoint = null;
            _registry.NewEndpointAdded += e => { endpoint = e; };

            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name");

            Assert.NotNull(endpoint);
            Assert.Equal("monitor", endpoint.MonitorType);
            Assert.Equal("address", endpoint.Address);
            Assert.Equal("name", endpoint.Name);
            Assert.Equal("group", endpoint.Group);
            Assert.Equal(id, endpoint.Id);
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_if_monitor_and_address_pair_is_different()
        {
            MockMonitor("monitor");
            MockMonitor("monitor1");

            var id1 = _registry.RegisterOrUpdate("monitor", "address", "group", "name");
            var id2 = _registry.RegisterOrUpdate("monitor1", "address", "group", "name");
            var id3 = _registry.RegisterOrUpdate("monitor", "address1", "group", "name");

            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id1, id3);
            Assert.NotEqual(id2, id3);
        }

        [Fact]
        public void RegisterOrUpdate_should_update_existing_endpoint_and_return_same_id_but_not_emit_NewEndpointAdded_event()
        {
            MockMonitor("monitor");
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name");

            Endpoint newEndpointCapture = null;
            _registry.NewEndpointAdded += e => { newEndpointCapture = e; };

            var id2 = _registry.RegisterOrUpdate("monitor", "ADDRESS", "group2", "name2");

            Assert.Equal(id, id2);
            Assert.Null(newEndpointCapture);

            var endpoint = _registry.GetById(id);
            Assert.NotNull(endpoint);
            Assert.Equal("monitor", endpoint.MonitorType);
            Assert.Equal("address", endpoint.Address);
            Assert.Equal("name2", endpoint.Name);
            Assert.Equal("group2", endpoint.Group);
        }

        [Fact]
        public void GetById_should_return_registered_endpoint()
        {
            MockMonitor("monitor");
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name");
            var endpoint = _registry.GetById(id);

            Assert.NotNull(endpoint);
            Assert.Equal("monitor", endpoint.MonitorType);
            Assert.Equal("address", endpoint.Address);
            Assert.Equal("name", endpoint.Name);
            Assert.Equal("group", endpoint.Group);
        }

        [Fact]
        public void TryUnregister_should_remove_endpoint_and_dispose_it()
        {
            MockMonitor("monitor");
            var id = _registry.RegisterOrUpdate("monitor", "address", "group", "name");
            var endpoint = _registry.GetById(id);
            Assert.True(_registry.TryUnregisterById(id), "Endpoint should be unregistered");
            Assert.True(endpoint.IsDisposed, "Endpoint should be disposed");
            Assert.Null(_registry.GetById(id));
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
            _monitorRegistry.Setup(r => r.FindByName("monitor")).Returns((IHealthMonitor)null);
            var exception = Assert.Throws<UnsupportedMonitorException>(() => _registry.RegisterOrUpdate("monitor", "a", "b", "c"));
            Assert.Equal("Unsupported monitor: monitor", exception.Message);
        }

        private void MockMonitor(string monitorType)
        {
            _monitorRegistry
                .Setup(r => r.FindByName(monitorType))
                .Returns(MonitorMock.Mock(monitorType));
        }
    }
}