using System;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointRegistryTests
    {
        private readonly EndpointRegistry _registry;

        public EndpointRegistryTests()
        {
            _registry = new EndpointRegistry();
        }

        [Fact]
        public void RegisterOrUpdate_should_register_new_endpoint_and_allow_to_retrieve_it()
        {
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
            var id = _registry.RegisterOrUpdate("proto", "address", "group", "name");
            var id2 = _registry.RegisterOrUpdate("proto", "address", "group2", "name2");

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
    }
}