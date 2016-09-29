using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public interface IEndpointMetricsForwarderCoordinator
    {
        void HandleMetricsForwarding(EndpointIdentity identity, EndpointMetadata metadata, EndpointHealth health);
    }
}