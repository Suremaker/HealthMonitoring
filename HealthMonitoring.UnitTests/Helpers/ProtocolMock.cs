using HealthMonitoring.Protocols;
using Moq;

namespace HealthMonitoring.UnitTests.Helpers
{
    static class ProtocolMock
    {
        public static IHealthCheckProtocol Mock(string name)
        {
            var proto = new Mock<IHealthCheckProtocol>();
            proto.Setup(p => p.Name).Returns(name);
            return proto.Object;
        }
    }
}