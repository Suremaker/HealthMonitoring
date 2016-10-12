using System;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class MonitorableEndpointRegistryTests
    {
        private readonly IMonitorableEndpointRegistry _registry;
        private readonly Mock<IHealthMonitorRegistry> _monitorRegistry;

        public MonitorableEndpointRegistryTests()
        {
            _monitorRegistry = new Mock<IHealthMonitorRegistry>();
            _registry = new MonitorableEndpointRegistry(_monitorRegistry.Object);
        }

        [Fact]
        public void TryRegister_should_register_new_endpoint_and_emit_NewEndpointAdded_event()
        {
            MockMonitor("monitor");

            MonitorableEndpoint capturedEndpoint = null;
            _registry.NewEndpointAdded += e => { capturedEndpoint = e; };

            var endpointIdentity = new EndpointIdentity(Guid.NewGuid(), "monitor", "address", "token1");
            var endpoint = _registry.TryRegister(endpointIdentity);

            Assert.NotNull(endpoint);
            Assert.Same(endpoint, capturedEndpoint);
            Assert.Equal("monitor", endpoint.Identity.MonitorType);
            Assert.Equal("address", endpoint.Identity.Address);
            Assert.Equal(endpointIdentity.Id, endpoint.Identity.Id);
        }

        [Fact]
        public void TryRegister_should_return_null_if_monitor_type_is_not_recognized()
        {
            var endpoint = _registry.TryRegister(new EndpointIdentity(Guid.NewGuid(), "unknownMonitorType", "address", "token1"));
            Assert.Null(endpoint);
        }

        [Fact]
        public void TryRegister_should_return_existing_endpoint_and_not_emit_NewEndpointAdded_event_if_endpoint_of_given_id_is_already_registered()
        {
            MockMonitor("monitor");
            var endpointId = Guid.NewGuid();
            var endpoint1 = _registry.TryRegister(new EndpointIdentity(endpointId, "monitor", "address", "token1"));

            MonitorableEndpoint newEndpointCapture = null;
            _registry.NewEndpointAdded += e => { newEndpointCapture = e; };

            var endpoint2 = _registry.TryRegister(new EndpointIdentity(endpointId, "monitor", "address", "token1"));
            Assert.Same(endpoint1, endpoint2);
            Assert.Null(newEndpointCapture);
        }

        [Fact]
        public void UpdateEndpoints_should_register_new_endpoints_dispose_not_specified_ones_and_ignore_ones_with_unrecognized_monitor_type()
        {
            MockMonitor("monitor");
            var e1 = _registry.TryRegister(new EndpointIdentity(Guid.NewGuid(), "monitor", "address", "token1"));
            var e2 = _registry.TryRegister(new EndpointIdentity(Guid.NewGuid(), "monitor", "address2", "token2"));
            var e3 = _registry.TryRegister(new EndpointIdentity(Guid.NewGuid(), "monitor", "address3", "token3"));
            var i4 = new EndpointIdentity(Guid.NewGuid(), "monitor", "address4", "token4");
            var i5 = new EndpointIdentity(Guid.NewGuid(), "monitor", "address4", "token4");
            var i6 = new EndpointIdentity(Guid.NewGuid(), "unknownMonitor", "address4", "token4");

            List<MonitorableEndpoint> captured = new List<MonitorableEndpoint>();
            _registry.NewEndpointAdded += e => captured.Add(e);

            _registry.UpdateEndpoints(e1.Identity, e2.Identity, i4, i5, i6);

            Assert.Equal(2, captured.Count);
            Assert.Contains(i4, captured.Select(c => c.Identity));
            Assert.Contains(i5, captured.Select(c => c.Identity));

            Assert.False(e1.IsDisposed, "e1.IsDisposed");
            Assert.False(e2.IsDisposed, "e2.IsDisposed");
            Assert.True(e3.IsDisposed, "e3.IsDisposed");
        }

        [Fact]
        public void TryUnregister_should_dispose_endpoint_it()
        {
            MockMonitor("monitor");
            var endpoint = _registry.TryRegister(new EndpointIdentity(Guid.NewGuid(), "monitor", "address", "token1"));
            Assert.True(_registry.TryUnregister(endpoint.Identity), "Endpoint should be unregistered");
            Assert.True(endpoint.IsDisposed, "Endpoint should be disposed");
        }

        [Fact]
        public void TryUnregister_should_return_false_if_endpoint_is_not_registered()
        {
            Assert.False(_registry.TryUnregister(new EndpointIdentity(Guid.NewGuid(), "monitor", "address", "token1")));
        }

        private void MockMonitor(string monitorType)
        {
            _monitorRegistry
                .Setup(r => r.FindByName(monitorType))
                .Returns(MonitorMock.Mock(monitorType));
        }
    }
}