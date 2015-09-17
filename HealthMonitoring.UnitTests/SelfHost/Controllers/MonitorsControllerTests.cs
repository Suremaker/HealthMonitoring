using HealthMonitoring.Monitors;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class MonitorsControllerTests
    {
        [Fact]
        public void GetTypes_should_return_all_registered_monitor_types()
        {
            var registry = new HealthMonitorRegistry(new[] { CreateMonitor("monitor2"), CreateMonitor("monitor1") });
            var controller = new MonitorsController(registry);
            Assert.Equal(new[] { "monitor1", "monitor2" }, controller.Get());
        }

        private IHealthMonitor CreateMonitor(string name)
        {
            var mock = new Mock<IHealthMonitor>();
            mock.Setup(m => m.Name).Returns(name);
            return mock.Object;
        }
    }
}
