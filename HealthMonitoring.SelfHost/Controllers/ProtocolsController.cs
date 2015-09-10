using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class ProtocolsController : ApiController
    {
        private readonly IProtocolRegistry _protocolRegistry;

        public ProtocolsController(IProtocolRegistry protocolRegistry)
        {
            _protocolRegistry = protocolRegistry;
        }

        [Route("api/protocols")]
        public IEnumerable<string> Get()
        {
            return _protocolRegistry.Protocols.Select(p => p.Name).OrderBy(p => p);
        }
    }
}