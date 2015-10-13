using System;
using System.Collections.Generic;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public interface IEndpointStatsRepository
    {
        void InsertEndpointStatistics(Guid endpointId, EndpointHealth stats);
        IEnumerable<EndpointStats> GetStatistics(Guid id, int limitDays);
        void DeleteStatistics(Guid id);
        void DeleteStatisticsOlderThan(DateTime date);
    }
}