using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using HealthMonitoring.Management.Core.Registers;
using Swashbuckle.Swagger.Annotations;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class MonitorsController : ApiController
    {
        private readonly IHealthMonitorTypeRegistry _healthMonitorTypeRegistry;

        public MonitorsController(IHealthMonitorTypeRegistry healthMonitorTypeRegistry)
        {
            _healthMonitorTypeRegistry = healthMonitorTypeRegistry;
        }

        [Route("api/monitors")]
        public IEnumerable<string> GetMonitorsTypes()
        {
            return _healthMonitorTypeRegistry.GetMonitorTypes().OrderBy(t => t);
        }

        [Route("api/monitors/register")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult PostRegisterMonitors([FromBody]params string[] monitorTypes)
        {
            if (monitorTypes == null || monitorTypes.Any(string.IsNullOrWhiteSpace))
                return BadRequest("Body cannot be null and have to contain properly named monitor types");

            foreach (var monitorType in monitorTypes)
                _healthMonitorTypeRegistry.RegisterMonitorType(monitorType);

            return Ok();
        }
    }
}