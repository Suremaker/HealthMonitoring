using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public class EndpointRegistry : IEndpointRegistry
    {
        private readonly ConcurrentDictionary<string, Endpoint> _endpoints = new ConcurrentDictionary<string, Endpoint>();
        private readonly ConcurrentDictionary<Guid, Endpoint> _endpointsByGuid = new ConcurrentDictionary<Guid, Endpoint>();
        public Guid RegisterOrUpdate(string protocol, string address, string group, string name)
        {
            string key = string.Format("{0}|{1}", protocol, address);
            var endpoint = _endpoints.AddOrUpdate(key, new Endpoint(Guid.NewGuid(), protocol, address, name, group), (k, e) => e.Update(group, name));
            _endpointsByGuid[endpoint.Id] = endpoint;
            return endpoint.Id;
        }

        public Endpoint GetById(Guid id)
        {
            Endpoint endpoint;
            return _endpointsByGuid.TryGetValue(id, out endpoint) ? endpoint : null;
        }

        public IEnumerable<Endpoint> Endpoints { get { return _endpoints.Select(p => p.Value); } }
    }
}