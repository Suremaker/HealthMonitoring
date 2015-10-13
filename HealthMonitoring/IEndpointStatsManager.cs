using System;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public interface IEndpointStatsManager
    {
        void RecordEndpointStatistics(Guid endpointId, EndpointHealth stats);
    }
}