using System;
using System.Web.Http;
using HealthMonitoring.SelfHost.Entities;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class EndpointsController : ApiController
    {
        [Route("api/endpoints/register")]
        public IHttpActionResult PostRegisterEndpoint([FromBody]EndpointRegistration endpoint)
        {
            endpoint.ValidateModel();
            var id = Guid.NewGuid();
            return Created(new Uri(Request.RequestUri, string.Format("/api/endpoints/{0}", id)), id);
        }
    }
}