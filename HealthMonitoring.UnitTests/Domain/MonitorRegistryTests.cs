using HealthMonitoring.Monitors;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class MonitorRegistryTests
    {
        [Fact]
        public void Monitors_should_return_all_monitors()
        {
            var monitors = new[] { CreateMonitor("monit1"), CreateMonitor("monit2") };
            Assert.Equal(monitors, new HealthMonitorRegistry(monitors).Monitors);
        }

        [Fact]
        public void FindByName_should_return_proper_monitor_instance()
        {
            var monitor = CreateMonitor("monit1");
            var registry = new HealthMonitorRegistry(new[] { monitor, CreateMonitor("monit2") });

            Assert.Same(monitor, registry.FindByName(monitor.Name));
        }

        private IHealthMonitor CreateMonitor(string name)
        {
            var mock = new Mock<IHealthMonitor>();
            mock.Setup(m => m.Name).Returns(name);
            return mock.Object;
        }
    }
}