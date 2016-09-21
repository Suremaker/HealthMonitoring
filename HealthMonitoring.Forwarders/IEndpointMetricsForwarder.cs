using System;

namespace HealthMonitoring.Forwarders
{
    public interface IEndpointMetricsForwarder
    {
        string Name { get; }
        void ForwardEndpointMetrics(Guid endpointId, EndpointMetrics metrics);
    }
}