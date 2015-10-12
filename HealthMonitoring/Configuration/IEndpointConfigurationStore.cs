using System;
using System.Collections.Generic;
using HealthMonitoring.Model;

namespace HealthMonitoring.Configuration
{
    public interface IEndpointConfigurationStore
    {
        void SaveEndpoint(Endpoint endpoint);
        void DeleteEndpoint(Guid endpointId);
        IEnumerable<Endpoint> LoadEndpoints(IHealthMonitorRegistry monitorRegistry);
    }
}
