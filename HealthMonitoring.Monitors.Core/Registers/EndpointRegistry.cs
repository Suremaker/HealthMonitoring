using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Registers
{
    public class MonitorableEndpointRegistry : IMonitorableEndpointRegistry
    {
        private static readonly ILog Logger = LogManager.GetLogger<MonitorableEndpointRegistry>();
        private readonly IHealthMonitorRegistry _healthMonitorRegistry;
        private readonly ConcurrentDictionary<Guid, MonitorableEndpoint> _endpoints = new ConcurrentDictionary<Guid, MonitorableEndpoint>();
        public event Action<MonitorableEndpoint> NewEndpointAdded;

        public MonitorableEndpointRegistry(IHealthMonitorRegistry healthMonitorRegistry)
        {
            _healthMonitorRegistry = healthMonitorRegistry;
        }

        public IEnumerable<MonitorableEndpoint> Endpoints => _endpoints.Values;

        public MonitorableEndpoint TryRegister(EndpointIdentity identity)
        {
            var monitor = _healthMonitorRegistry.FindByName(identity.MonitorType);
            if (monitor == null)
                return null;

            var monitorableEndpoint = new MonitorableEndpoint(identity, monitor);

            var current = _endpoints.GetOrAdd(identity.Id, monitorableEndpoint);

            if (ReferenceEquals(current, monitorableEndpoint))
            {
                Logger.Info($"Starting monitoring of: {monitorableEndpoint}");
                NewEndpointAdded?.Invoke(monitorableEndpoint);
            }
            return current;
        }

        public bool TryUnregister(EndpointIdentity identity)
        {
            MonitorableEndpoint removedEndpoint;
            if (!_endpoints.TryRemove(identity.Id, out removedEndpoint))
                return false;

            removedEndpoint.Dispose();
            Logger.Info($"Stopped monitoring of: {removedEndpoint}");
            return true;
        }

        public void UpdateEndpoints(EndpointIdentity[] identities)
        {
            foreach (var endpoint in identities)
                TryRegister(endpoint);

            foreach (var removed in Endpoints.Where(e => identities.All(i => i.Id != e.Identity.Id)))
                TryUnregister(removed.Identity);
        }
    }
}