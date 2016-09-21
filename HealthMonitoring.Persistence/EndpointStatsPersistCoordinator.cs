using System;
using System.Collections.Generic;
using HealthMonitoring.Forwarders;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;

namespace HealthMonitoring.Persistence
{
    public class EndpointStatsPersistCoordinator : IEndpointStatsPersistCoordinator
    {
        private readonly IEndpointStatsRepository _defaultRepository;
        private readonly IDictionary<string, IEndpointMetricsForwarder> _forwarders = new Dictionary<string, IEndpointMetricsForwarder>();

        public EndpointStatsPersistCoordinator(IEndpointStatsRepository defaultRepository, IEnumerable<IEndpointMetricsForwarder> forwarders)
        {
            _defaultRepository = defaultRepository;

            foreach (var forwarder in forwarders)
            {
                if (!_forwarders.ContainsKey(forwarder.Name))
                    _forwarders.Add(forwarder.Name, forwarder);
            }
        }

        public void InsertEndpointStatistics(Guid endpointId, EndpointHealth stats)
        {
            _defaultRepository.InsertEndpointStatistics(endpointId, stats);

            foreach (var f in _forwarders)
            {
                f.Value.ForwardEndpointMetrics(endpointId, new EndpointMetrics(stats.CheckTimeUtc, stats.ResponseTime.Ticks, stats.Status.ToString()));
            }
        }

        public IEnumerable<EndpointStats> GetStatistics(Guid id, int limitDays)
        {
            return _defaultRepository.GetStatistics(id, limitDays);
        }

        public void DeleteStatistics(Guid id)
        {
            _defaultRepository.DeleteStatistics(id);
        }

        public void DeleteStatisticsOlderThan(DateTime date)
        {
            _defaultRepository.DeleteStatisticsOlderThan(date);
        }
    }
}
