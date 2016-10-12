using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public interface IEndpointStatsManager
    {
        void RecordEndpointStatistics(EndpointIdentity identity, EndpointMetadata metadata, EndpointHealth stats);
    }
}