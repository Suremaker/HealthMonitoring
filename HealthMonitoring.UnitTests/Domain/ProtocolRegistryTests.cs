using HealthMonitoring.Protocols;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class ProtocolRegistryTests
    {
        [Fact]
        public void Protocols_should_return_all_protocols()
        {
            var protocols = new[] { CreateProtocol("proto1"), CreateProtocol("proto2") };
            Assert.Equal(protocols, new ProtocolRegistry(protocols).Protocols);
        }

        [Fact]
        public void FindByName_should_return_proper_protocol_instance()
        {
            var protocol = CreateProtocol("proto1");
            var registry = new ProtocolRegistry(new[] { protocol, CreateProtocol("proto2") });

            Assert.Same(protocol, registry.FindByName(protocol.Name));
        }

        private IHealthCheckProtocol CreateProtocol(string name)
        {
            var mock = new Mock<IHealthCheckProtocol>();
            mock.Setup(m => m.Name).Returns(name);
            return mock.Object;
        }
    }
}