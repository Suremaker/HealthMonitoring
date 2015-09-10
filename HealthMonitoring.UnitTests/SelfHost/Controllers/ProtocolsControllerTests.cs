using HealthMonitoring.Protocols;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class ProtocolsControllerTests
    {
        [Fact]
        public void GetTypes_should_return_all_registered_protocol_types()
        {
            var registry = new ProtocolRegistry(new[] { CreateProtocol("proto2"), CreateProtocol("proto1") });
            var controller = new ProtocolsController(registry);
            Assert.Equal(new[] { "proto1", "proto2" }, controller.Get());
        }

        private IHealthCheckProtocol CreateProtocol(string name)
        {
            var mock = new Mock<IHealthCheckProtocol>();
            mock.Setup(m => m.Name).Returns(name);
            return mock.Object;
        }
    }
}
