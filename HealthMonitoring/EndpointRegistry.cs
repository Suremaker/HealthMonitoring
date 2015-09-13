using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public class EndpointRegistry : IEndpointRegistry
    {
        private readonly IProtocolRegistry _protocolRegistry;
        private readonly ConcurrentDictionary<string, Endpoint> _endpoints = new ConcurrentDictionary<string, Endpoint>();
        private readonly ConcurrentDictionary<Guid, Endpoint> _endpointsByGuid = new ConcurrentDictionary<Guid, Endpoint>();

        public IEnumerable<Endpoint> Endpoints { get { return _endpoints.Select(p => p.Value); } }

        public EndpointRegistry(IProtocolRegistry protocolRegistry)
        {
            _protocolRegistry = protocolRegistry;
        }

        public Guid RegisterOrUpdate(string protocol, string address, string group, string name)
        {
            var proto = _protocolRegistry.FindByName(protocol);
            if (proto == null)
                throw new UnsupportedProtocolException(protocol);
            var key = GetKey(protocol, address);
            var endpoint = _endpoints.AddOrUpdate(key, new Endpoint(Guid.NewGuid(), protocol, address, name, group), (k, e) => e.Update(group, name));
            _endpointsByGuid[endpoint.Id] = endpoint;
            return endpoint.Id;
        }

        public Endpoint GetById(Guid id)
        {
            Endpoint endpoint;
            return _endpointsByGuid.TryGetValue(id, out endpoint) ? endpoint : null;
        }

        private static string GetKey(string protocol, string address)
        {
            return string.Format("{0}|{1}", protocol, address.ToLowerInvariant());
        }
    }
}