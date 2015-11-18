using System.Collections.Generic;
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
            var throttlingSettings = new Mock<IThrottlingSettings>();
            var throttling = new Dictionary<string, int>();
            throttlingSettings.Setup(t => t.Throttling).Returns(throttling);
            var controller = new ConfigController(monitor.Object, dashboard.Object, throttlingSettings.Object);
            var config = controller.GetConfig();
            Assert.NotNull(config);
            Assert.Same(monitor.Object, config.Monitor);
            Assert.Same(dashboard.Object, config.Dashboard);
            Assert.Same(throttling, config.Throttling);
        }
    }
}
