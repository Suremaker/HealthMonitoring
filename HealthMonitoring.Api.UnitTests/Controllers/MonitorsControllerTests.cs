using System.Web.Http.Results;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Controllers
{
    public class MonitorsControllerTests
    {
        [Fact]
        public void GetMonitorTypes_should_return_all_registered_monitor_types()
        {
            var registry = new Mock<IHealthMonitorTypeRegistry>();
            registry.Setup(r => r.GetMonitorTypes()).Returns(new[] { "monitor2", "monitor1" });
            var controller = new MonitorsController(registry.Object);
            Assert.Equal(new[] { "monitor1", "monitor2" }, controller.GetMonitorsTypes());
        }

        [Fact]
        public void PostRegisterMonitors_should_register_unspecified_monitor_types()
        {
            var registry = new Mock<IHealthMonitorTypeRegistry>();
            var controller = new MonitorsController(registry.Object);
            Assert.IsType<OkResult>(controller.PostRegisterMonitors("monitor1", "monitor2"));
            registry.Verify(r => r.RegisterMonitorType("monitor1"));
            registry.Verify(r => r.RegisterMonitorType("monitor2"));
        }
    }
}
