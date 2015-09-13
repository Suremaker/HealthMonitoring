using System;
using HealthMonitoring.Protocols;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointRegistryTests
    {
        private readonly EndpointRegistry _registry;
        private readonly Mock<IProtocolRegistry> _protocolRegistry;

        public EndpointRegistryTests()
        {
            _protocolRegistry = new Mock<IProtocolRegistry>();
            _registry = new EndpointRegistry(_protocolRegistry.Object);
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_and_allow_to_retrieve_it()
        {
            MockProtocol("proto");
            var id = _registry.RegisterOrUpdate("proto", "address", "group", "name");
            var endpoint = _registry.GetById(id);

            Assert.NotNull(endpoint);
            Assert.Equal("proto", endpoint.Protocol);
            Assert.Equal("address", endpoint.Address);
            Assert.Equal("name", endpoint.Name);
            Assert.Equal("group", endpoint.Group);
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_if_protocol_and_address_pair_is_different()
        {
            MockProtocol("proto");
            MockProtocol("proto1");

            var id1 = _registry.RegisterOrUpdate("proto", "address", "group", "name");
            var id2 = _registry.RegisterOrUpdate("proto1", "address", "group", "name");
            var id3 = _registry.RegisterOrUpdate("proto", "address1", "group", "name");

            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id1, id3);
            Assert.NotEqual(id2, id3);
        }

        [Fact]
        public void RegisterOrUpdate_should_update_existing_endpoint_and_return_same_id()
        {
            MockProtocol("proto");
            var id = _registry.RegisterOrUpdate("proto", "address", "group", "name");
            var id2 = _registry.RegisterOrUpdate("proto", "ADDRESS", "group2", "name2");

            Assert.Equal(id, id2);

            var endpoint = _registry.GetById(id);
            Assert.NotNull(endpoint);
            Assert.Equal("proto", endpoint.Protocol);
            Assert.Equal("address", endpoint.Address);
            Assert.Equal("name2", endpoint.Name);
            Assert.Equal("group2", endpoint.Group);
        }

        [Fact]
        public void GetById_shoulr_return_null_for_unknown_id()
        {
            Assert.Null(_registry.GetById(Guid.NewGuid()));
        }

        [Fact]
        public void RegisterOrUpdate_should_throw_UnsupportedProtocolException_if_protocol_is_not_recognized()
        {
            _protocolRegistry.Setup(r => r.FindByName("proto")).Returns((IHealthCheckProtocol)null);
            var exception = Assert.Throws<UnsupportedProtocolException>(() => _registry.RegisterOrUpdate("proto", "a", "b", "c"));
            Assert.Equal("Unsupported protocol: proto", exception.Message);
        }

        private void MockProtocol(string protocolName)
        {
            _protocolRegistry
                .Setup(r => r.FindByName(protocolName))
                .Returns(new Mock<IHealthCheckProtocol>().Object);
        }
    }
}