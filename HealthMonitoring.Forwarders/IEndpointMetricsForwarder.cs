using System;

namespace HealthMonitoring.Forwarders
{
    public interface IEndpointMetricsForwarder
    {
        void ForwardEndpointMetrics(Guid endpointId, EndpointMetrics metrics);
    }
}