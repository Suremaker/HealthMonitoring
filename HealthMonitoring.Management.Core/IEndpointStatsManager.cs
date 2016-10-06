using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public interface IEndpointStatsManager
    {
        void RecordEndpointStatistics(Guid endpointId, EndpointHealth stats);
    }
}