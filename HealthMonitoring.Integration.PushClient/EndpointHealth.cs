using System.Collections.Generic;

namespace HealthMonitoring.Integration.PushClient
{
    public class EndpointHealth
    {
        public HealthStatus Status { get; }
        public IReadOnlyDictionary<string, string> Details { get; }

        public EndpointHealth(HealthStatus status, IReadOnlyDictionary<string, string> details = null)
        {
            Status = status;
            Details = details ?? new Dictionary<string, string>();
        }
    }
}