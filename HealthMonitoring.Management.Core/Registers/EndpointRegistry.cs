﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;
using HealthMonitoring.Security;

namespace HealthMonitoring.Management.Core.Registers
{
    public class EndpointRegistry : IEndpointRegistry
    {
        private readonly IHealthMonitorTypeRegistry _healthMonitorTypeRegistry;
        private readonly IEndpointConfigurationRepository _endpointConfigurationRepository;
        private readonly IEndpointStatsRepository _statsRepository;
        private readonly IEndpointStatsManager _endpointStatsManager;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly ConcurrentDictionary<string, Endpoint> _endpoints = new ConcurrentDictionary<string, Endpoint>();
        private readonly ConcurrentDictionary<Guid, Endpoint> _endpointsByGuid = new ConcurrentDictionary<Guid, Endpoint>();
        private readonly ICredentialsProvider _credentialsProvider;

        public IEnumerable<Endpoint> Endpoints { get { return _endpoints.Select(p => p.Value); } }

        public EndpointRegistry(
            IHealthMonitorTypeRegistry healthMonitorTypeRegistry, 
            IEndpointConfigurationRepository endpointConfigurationRepository,
            IEndpointStatsRepository statsRepository,
            IEndpointStatsManager statsManager,
            ITimeCoordinator timeCoordinator,
            ICredentialsProvider credentialsProvider)
        {
            _healthMonitorTypeRegistry = healthMonitorTypeRegistry;
            _endpointConfigurationRepository = endpointConfigurationRepository;
            _statsRepository = statsRepository;
            _endpointStatsManager = statsManager;
            _timeCoordinator = timeCoordinator;
            _credentialsProvider = credentialsProvider;

            foreach (var endpoint in _endpointConfigurationRepository.LoadEndpoints())
            {
                if (_endpoints.TryAdd(endpoint.Identity.GetNaturalKey(), endpoint))
                    _endpointsByGuid.TryAdd(endpoint.Identity.Id, endpoint);
            }
        }

        public Guid RegisterOrUpdate(string monitorType, string address, string group, string name, string[] tags, string privateToken = null)
        {
            if (!_healthMonitorTypeRegistry.GetMonitorTypes().Contains(monitorType))
                throw new UnsupportedMonitorException(monitorType);
            var encryptedToken = privateToken?.ToSha256Hash();
            var newIdentifier = new EndpointIdentity(Guid.NewGuid(), monitorType, address, encryptedToken);
            var endpoint = _endpoints.AddOrUpdate(newIdentifier.GetNaturalKey(),
                                new Endpoint(_timeCoordinator, newIdentifier, new EndpointMetadata(name, group, tags)),
                                (k, e) => e.UpdateEndpoint(group, name, tags, encryptedToken));
            _endpointsByGuid[endpoint.Identity.Id] = endpoint;
            _endpointConfigurationRepository.SaveEndpoint(endpoint);

            if (endpoint.Identity == newIdentifier)
                EndpointAdded?.Invoke(endpoint);
            return endpoint.Identity.Id;
        }

        public bool TryUpdateEndpointTags(Guid id, string[] tags)
        {
            Endpoint endpoint;
            if (!_endpointsByGuid.TryGetValue(id, out endpoint))
                return false;

            var metadata = endpoint.Metadata.Name;
            endpoint.UpdateEndpoint(endpoint.Metadata.Group, metadata, tags);
            _endpointConfigurationRepository.SaveEndpoint(endpoint);
            return true;
        }

        public Endpoint GetById(Guid id)
        {
            Endpoint endpoint;
            return _endpointsByGuid.TryGetValue(id, out endpoint) ? endpoint : null;
        }

        public Endpoint GetByNaturalKey(string key)
        {
            return _endpoints.FirstOrDefault(m => m.Key == key).Value;
        }

        public bool TryUnregisterById(Guid id)
        {
            Endpoint endpoint;

            if (!_endpointsByGuid.TryRemove(id, out endpoint) ||
                !_endpoints.TryRemove(endpoint.Identity.GetNaturalKey(), out endpoint))
                return false;

            endpoint.Dispose();

            _endpointConfigurationRepository.DeleteEndpoint(endpoint.Identity.Id);
            _statsRepository.DeleteStatistics(endpoint.Identity.Id);
            return true;
        }

        public void UpdateHealth(Guid endpointId, EndpointHealth health)
        {
            var endpoint = GetById(endpointId);
            if (endpoint == null)
                return;
            endpoint.UpdateHealth(health);
            _endpointStatsManager.RecordEndpointStatistics(endpoint.Identity, endpoint.Metadata, health);
        }

        public event Action<Endpoint> EndpointAdded;
    }
}