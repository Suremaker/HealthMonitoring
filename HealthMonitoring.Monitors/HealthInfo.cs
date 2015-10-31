using System.Collections.Generic;

namespace HealthMonitoring.Monitors
{
    public class HealthInfo
    {
        public HealthInfo(HealthStatus status, IReadOnlyDictionary<string, string> details = null)
        {
            Status = status;
            Details = details ?? new Dictionary<string, string>();
        }

        public HealthStatus Status { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }
    }
}