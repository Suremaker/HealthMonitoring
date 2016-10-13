using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Controllers;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Controllers
{
    public class MonitorsControllerTests
    {
        private readonly Mock<IHealthMonitorTypeRegistry> _registry;
        private readonly MonitorsController _controller;

        public MonitorsControllerTests()
        {
            _registry = new Mock<IHealthMonitorTypeRegistry>();
            SetUpReqistry();
            _controller = new MonitorsController(_registry.Object);  
        }

        private void SetUpReqistry()
        {
            _registry.Setup(r => r.GetMonitorTypes()).Returns(new[] { "monitor2", "monitor1" });
        }

        [Fact]
        public void GetMonitorTypes_should_return_all_registered_monitor_types()
        {
            Assert.Equal(new[] { "monitor1", "monitor2" }, _controller.GetMonitorsTypes());
        }

        [Fact]
        public void PostRegisterMonitors_should_register_unspecified_monitor_types()
        {
            AuthorizeRequest(SecurityRole.PullMonitor);
            Assert.IsType<OkResult>(_controller.PostRegisterMonitors("monitor1", "monitor2"));
            _registry.Verify(r => r.RegisterMonitorType("monitor1"));
            _registry.Verify(r => r.RegisterMonitorType("monitor2"));
        }

        private void AuthorizeRequest(params SecurityRole[] roles)
        {
            var identity = new GenericIdentity(Guid.NewGuid().ToString());
            var principal = new GenericPrincipal(identity, roles.Select(m => m.ToString()).ToArray());
            _controller.RequestContext = new HttpRequestContext
            {
                Principal = principal
            };
        }
    }
}
