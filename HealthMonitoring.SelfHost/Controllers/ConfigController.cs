using System.Web.Http;
using HealthMonitoring.Configuration;
using HealthMonitoring.SelfHost.Entities;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class ConfigController : ApiController
    {
        private readonly Config _config;

        public ConfigController(IMonitorSettings monitorSettings, IDashboardSettings dashboardSettings, IThrottlingSettings throttlingSettings)
        {
            _config = new Config(monitorSettings, dashboardSettings, throttlingSettings);
        }

        [Route("api/config")]
        public Config GetConfig()
        {
            return _config;
        }
    }
}