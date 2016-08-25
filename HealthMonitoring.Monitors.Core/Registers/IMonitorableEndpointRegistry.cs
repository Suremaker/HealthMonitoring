using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Registers
{
    public interface IMonitorableEndpointRegistry
    {
        event Action<MonitorableEndpoint> NewEndpointAdded;
        void UpdateEndpoints(params EndpointIdentity[] identities);
        MonitorableEndpoint TryRegister(EndpointIdentity identity);
        bool TryUnregister(EndpointIdentity identity);
    }
}