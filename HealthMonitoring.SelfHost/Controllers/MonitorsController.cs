using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class MonitorsController : ApiController
    {
        private readonly IHealthMonitorRegistry _healthMonitorRegistry;

        public MonitorsController(IHealthMonitorRegistry healthMonitorRegistry)
        {
            _healthMonitorRegistry = healthMonitorRegistry;
        }

        [Route("api/monitors")]
        public IEnumerable<string> Get()
        {
            return _healthMonitorRegistry.Monitors.Select(p => p.Name).OrderBy(p => p);
        }
    }
}