using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using HealthMonitoring.SelfHost.Entities;
using Swashbuckle.Swagger.Annotations;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class EndpointsController : ApiController
    {
        private readonly IEndpointRegistry _endpointRegistry;
        private readonly IEndpointStatsRepository _endpointStatsRepository;

        public EndpointsController(IEndpointRegistry endpointRegistry, IEndpointStatsRepository endpointStatsRepository)
        {
            _endpointRegistry = endpointRegistry;
            _endpointStatsRepository = endpointStatsRepository;
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
                var id = _endpointRegistry.RegisterOrUpdate(endpoint.MonitorType, endpoint.Address, endpoint.Group, endpoint.Name);

                return Created(new Uri(Request.RequestUri, string.Format("/api/endpoints/{0}", id)), id);
            }
            catch (UnsupportedMonitorException e)
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

        [Route("api/endpoints/{id}/stats")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EndpointHealthStats[]))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public EndpointHealthStats[] GetEndpointStats(Guid id, int? limitDays = null)
        {
            return _endpointStatsRepository.GetStatistics(id, limitDays.GetValueOrDefault(1)).Select(EndpointHealthStats.FromDomain).ToArray();
        }

        [Route("api/endpoints/{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteEndpoint(Guid id)
        {
            if (_endpointRegistry.TryUnregisterById(id))
                return Ok();
            return NotFound();
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