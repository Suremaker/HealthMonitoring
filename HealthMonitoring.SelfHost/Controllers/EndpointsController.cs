using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using HealthMonitoring.Model;
using HealthMonitoring.SelfHost.Entities;
using Swashbuckle.Swagger.Annotations;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class EndpointsController : ApiController
    {
        private readonly IEndpointRegistry _endpointRegistry;

        public EndpointsController(IEndpointRegistry endpointRegistry)
        {
            _endpointRegistry = endpointRegistry;
        }

        [Route("api/endpoints/register")]
        [ResponseType(typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult PostRegisterEndpoint([FromBody]EndpointRegistration endpoint)
        {
            endpoint.ValidateModel();
            try
            {
                var id = _endpointRegistry.RegisterOrUpdate(endpoint.Protocol, endpoint.Address, endpoint.Group, endpoint.Name);

                return Created(new Uri(Request.RequestUri, string.Format("/api/endpoints/{0}", id)), id);
            }
            catch (UnsupportedProtocolException e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("api/endpoints/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EndpointDetails))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [ResponseType(typeof(EndpointDetails))]
        public IHttpActionResult GetEndpoint(Guid id)
        {
            var endpoint = _endpointRegistry.GetById(id);
            if (endpoint == null)
                return NotFound();
            return Ok(new EndpointDetails(endpoint));
        }

        [Route("api/endpoints")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EndpointDetails))]
        [ResponseType(typeof(EndpointDetails))]
        public IEnumerable<EndpointDetails> GetEndpoints()
        {
            return _endpointRegistry.Endpoints.Select(e => new EndpointDetails(e));
        }
    }
}