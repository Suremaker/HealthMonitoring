using System;
using HealthMonitoring.Protocols;
using Moq;

namespace HealthMonitoring.UnitTests.Helpers
{
    static class ProtocolMock
    {
        public static IHealthCheckProtocol Mock(string name)
        {
            return GetMock(name).Object;
        }

        public static Mock<IHealthCheckProtocol> GetMock(string name)
        {
            var proto = new Mock<IHealthCheckProtocol>();
            proto.Setup(p => p.Name).Returns(name);
            return proto;
        }
    }
}