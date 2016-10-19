﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Entities;
using HealthMonitoring.SelfHost.Filters;
using HealthMonitoring.SelfHost.Security;
using HealthMonitoring.TimeManagement;
using Swashbuckle.Swagger.Annotations;

namespace HealthMonitoring.SelfHost.Controllers
{
    [EnableCors(methods: "GET", origins: "*", headers: "*")]
    public class EndpointsController : ApiController
    {
        private readonly IEndpointRegistry _endpointRegistry;
        private readonly IEndpointStatsRepository _endpointStatsRepository;
        private readonly ITimeCoordinator _timeCoordinator;

        public EndpointsController(IEndpointRegistry endpointRegistry, IEndpointStatsRepository endpointStatsRepository, ITimeCoordinator timeCoordinator)
        {
            _endpointRegistry = endpointRegistry;
            _endpointStatsRepository = endpointStatsRepository;
            _timeCoordinator = timeCoordinator;
        }

        [Route("api/endpoints/register")]
        [ResponseType(typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostRegisterEndpoint([FromBody]EndpointRegistration endpoint)
        {
            endpoint.ValidateModel();

            try
            {
                var existed = _endpointRegistry.GetByNaturalKey(endpoint.MonitorType, endpoint.Address);
                RequestContext.AuthorizeRegistration(endpoint, existed, SecurityRole.Admin);

                var id = _endpointRegistry.RegisterOrUpdate(endpoint.MonitorType, endpoint.Address, endpoint.Group, endpoint.Name, endpoint.Tags, endpoint.Password);
                return Created(new Uri(Request.RequestUri, $"/api/endpoints/{id}"), id);
            }
            catch (UnsupportedMonitorException e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("api/endpoints/health")]
        [ResponseType(typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostEndpointsHealth(DateTimeOffset? clientCurrentTime = null, [FromBody]params EndpointHealthUpdate[] healthUpdate)
        {
            RequestContext.Authorize(SecurityRole.Monitor);
            healthUpdate.ValidateModel();

            var clockDifference = GetServerToClientTimeDifference(clientCurrentTime);

            foreach (var update in healthUpdate)
                _endpointRegistry.UpdateHealth(update.EndpointId, update.ToEndpointHealth(clockDifference));
            return Ok();
        }

        [Route("api/endpoints/{id}/health")]
        [ResponseType(typeof(Guid))]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostEndpointHealth(Guid id, [FromBody]HealthUpdate health, DateTimeOffset? clientCurrentTime = null)
        {
            if (_endpointRegistry.GetById(id) != null)
                RequestContext.Authorize(id);

            health.ValidateModel();
            var clockDifference = GetServerToClientTimeDifference(clientCurrentTime);

            if (!_endpointRegistry.UpdateHealth(id, health.ToEndpointHealth(clockDifference)))
                return NotFound();

            return Ok();
        }

        private TimeSpan GetServerToClientTimeDifference(DateTimeOffset? clientCurrentTime)
        {
            var serverCurrentTime = (DateTimeOffset)_timeCoordinator.UtcNow;
            var clientTime = (clientCurrentTime ?? serverCurrentTime).ToUniversalTime();
            return serverCurrentTime - clientTime;
        }

        [Route("api/endpoints/identities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EndpointIdentity[]))]
        [ResponseType(typeof(EndpointIdentity[]))]
        public EndpointIdentity[] GetEndpointsIdentities()
        {
            return _endpointRegistry.Endpoints.Select(e => e.Identity).ToArray();
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
            return Ok(EndpointDetails.FromDomain(endpoint));
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
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteEndpoint(Guid id)
        {
            RequestContext.Authorize(id, SecurityRole.Admin);

            if (_endpointRegistry.TryUnregisterById(id))
                return Ok();

            return NotFound();
        }

        [Route("api/endpoints")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EndpointDetails[]))]
        [ResponseType(typeof(EndpointDetails[]))]
        public IEnumerable<EndpointDetails> GetEndpoints([FromUri]string[] filterStatus = null, [FromUri]string[] filterTags = null, string filterGroup = null, string filterText = null)
        {
            var filter = new EndpointFilter()
                .WithGroup(filterGroup)
                .WithStatus(filterStatus)
                .WithTags(filterTags)
                .WithText(filterText);
            return _endpointRegistry.Endpoints.Select(EndpointDetails.FromDomain).Where(filter.DoesMatch);
        }

        [Route("api/endpoints/{id}/tags")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [ResponseType(typeof(EndpointDetails))]
        public IHttpActionResult PutUpdateEndpointTags(Guid id, [FromBody]string[] tags)
        {
            try
            {
                RequestContext.Authorize(id, SecurityRole.Admin);

                tags.CheckForUnallowedSymbols();

                if (_endpointRegistry.TryUpdateEndpointTags(id, tags))
                    return Ok();

                return NotFound();
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}