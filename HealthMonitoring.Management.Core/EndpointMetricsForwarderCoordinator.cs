using System.Collections.Generic;
using Common.Logging;
using HealthMonitoring.Forwarders;

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

        public void HandleMetricsForwarding(Endpoint endpoint)
        {
            foreach (var f in _forwarders)
            {
                Logger.InfoFormat("Forwarding metrics using {0} forwarder, for endpoint id {1}", f.Key, endpoint.Identity.Id);

                f.Value.ForwardEndpointMetrics(
                    new EndpointDetails(
                        endpoint.Identity.Id,
                        endpoint.Metadata.Group,
                        endpoint.Metadata.Name,
                        endpoint.Identity.Address,
                        endpoint.Identity.MonitorType),
                    new EndpointMetrics(
                        endpoint.Health.CheckTimeUtc,
                        endpoint.Health.ResponseTime.Milliseconds,
                        endpoint.Health.Status.ToString()));
            }
        }
    }
}