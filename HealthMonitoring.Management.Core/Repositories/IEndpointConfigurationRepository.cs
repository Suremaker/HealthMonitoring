using System;
using System.Collections.Generic;

namespace HealthMonitoring.Management.Core.Repositories
{
    public interface IEndpointConfigurationRepository
    {
        void SaveEndpoint(Endpoint endpoint);
        void DeleteEndpoint(Guid endpointId);
        IEnumerable<Endpoint> LoadEndpoints();
    }
}
