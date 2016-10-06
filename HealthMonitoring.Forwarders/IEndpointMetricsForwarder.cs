namespace HealthMonitoring.Forwarders
{
    public interface IEndpointMetricsForwarder
    {
        void ForwardEndpointMetrics(EndpointDetails details , EndpointMetrics metrics);
    }
}