using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public interface IEndpointMetricsForwarderCoordinator
    {
        void HandleMetricsForwarding(Guid endpointId, EndpointHealth stats);
    }
}