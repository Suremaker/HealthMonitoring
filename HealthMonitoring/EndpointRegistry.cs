using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public class EndpointRegistry : IEndpointRegistry
    {
        private readonly IHealthMonitorRegistry _healthMonitorRegistry;
        private readonly ConcurrentDictionary<string, Endpoint> _endpoints = new ConcurrentDictionary<string, Endpoint>();
        private readonly ConcurrentDictionary<Guid, Endpoint> _endpointsByGuid = new ConcurrentDictionary<Guid, Endpoint>();

        public IEnumerable<Endpoint> Endpoints { get { return _endpoints.Select(p => p.Value); } }
        public event Action<Endpoint> NewEndpointAdded;

        public EndpointRegistry(IHealthMonitorRegistry healthMonitorRegistry)
        {
            _healthMonitorRegistry = healthMonitorRegistry;
        }

        public Guid RegisterOrUpdate(string monitorType, string address, string group, string name)
        {
            var monitor = _healthMonitorRegistry.FindByName(monitorType);
            if (monitor == null)
                throw new UnsupportedMonitorException(monitorType);

            var key = GetKey(monitorType, address);
            var newId = Guid.NewGuid();
            var endpoint = _endpoints.AddOrUpdate(key, new Endpoint(newId, monitor, address, name, group), (k, e) => e.Update(group, name));
            _endpointsByGuid[endpoint.Id] = endpoint;

            if (endpoint.Id == newId && NewEndpointAdded != null)
                NewEndpointAdded(endpoint);

            return endpoint.Id;
        }

        public Endpoint GetById(Guid id)
        {
            Endpoint endpoint;
            return _endpointsByGuid.TryGetValue(id, out endpoint) ? endpoint : null;
        }

        public bool TryUnregisterById(Guid id)
        {
            Endpoint endpoint;

            if (!_endpointsByGuid.TryRemove(id, out endpoint) ||
                !_endpoints.TryRemove(GetKey(endpoint.MonitorType, endpoint.Address), out endpoint))
                return false;

            endpoint.Dispose();
            return true;
        }

        private static string GetKey(string monitor, string address)
        {
            return string.Format("{0}|{1}", monitor, address.ToLowerInvariant());
        }
    }
}