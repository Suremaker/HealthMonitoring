using System;
using System.Collections.Generic;
using Common.Logging;
using HealthMonitoring.Forwarders;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core
{
    public class EndpointMetricsForwarderCoordinator : IEndpointMetricsForwarderCoordinator
    {
        private static readonly ILog Logger = LogManager.GetLogger<EndpointMetricsForwarderCoordinator>();

        private readonly IDictionary<string, IEndpointMetricsForwarder> _forwarders = new Dictionary<string, IEndpointMetricsForwarder>();

        public EndpointMetricsForwarderCoordinator(IEnumerable<IEndpointMetricsForwarder> forwarders)
        {
            foreach (var forwarder in forwarders)
            {
                var type = forwarder.GetType().ToString();
                if (!_forwarders.ContainsKey(type))
                    _forwarders.Add(type, forwarder);
                else
                    Logger.WarnFormat("Forwarder with type {0} already exists and it is not going to be registered", type);
            }
        }
        
        public void HandleMetricsForwarding(Guid endpointId, EndpointHealth stats)
        {
            foreach (var f in _forwarders)
            {
                Logger.InfoFormat("Forwarding metrics using {0} forwarder", f.Key);
                f.Value.ForwardEndpointMetrics(endpointId, new EndpointMetrics(stats.CheckTimeUtc, stats.ResponseTime.Ticks, stats.Status.ToString()));
            }
        }
    }
}