using HealthMonitoring.Configuration;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class ConfigControllerTests
    {
        [Fact]
        public void GetConfig_should_return_configuration()
        {
            var monitor = new Mock<IMonitorSettings>();
            var dashboard = new Mock<IDashboardSettings>();
            var controller = new ConfigController(monitor.Object, dashboard.Object);
            var config = controller.GetConfig();
            Assert.NotNull(config);
            Assert.Same(monitor.Object, config.Monitor);
            Assert.Same(dashboard.Object, config.Dashboard);
        }
    }
}
