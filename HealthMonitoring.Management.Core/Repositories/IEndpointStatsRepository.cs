using System;
using System.Collections.Generic;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core.Repositories
{
    public delegate void EndpointStatisticInsertedEventhandler(Guid id, EndpointHealth stats);

    public interface IEndpointStatsRepository
    {
        event EndpointStatisticInsertedEventhandler EndpointStatisticsInserted;
        void InsertEndpointStatistics(Guid endpointId, EndpointHealth stats);
        IEnumerable<EndpointStats> GetStatistics(Guid id, int limitDays);
        void DeleteStatistics(Guid id);
        void DeleteStatisticsOlderThan(DateTime date);
    }
}