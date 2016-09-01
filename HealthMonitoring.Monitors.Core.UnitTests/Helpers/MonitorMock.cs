using Moq;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers
{
    static class MonitorMock
    {
        public static IHealthMonitor Mock(string name)
        {
            return GetMock(name).Object;
        }

        public static Mock<IHealthMonitor> GetMock(string name)
        {
            var monitor = new Mock<IHealthMonitor>();
            monitor.Setup(p => p.Name).Returns(name);
            return monitor;
        }
    }
}