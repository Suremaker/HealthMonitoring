using System;
using System.Collections.Generic;

namespace HealthMonitoring.Model
{
    public class EndpointHealth
    {
        public TimeSpan ResponseTime { get; private set; }
        public EndpointStatus Status { get; private set; }
        public DateTime CheckTimeUtc { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }

        public EndpointHealth(DateTime checkTimeUtc, TimeSpan responseTime, EndpointStatus status, IReadOnlyDictionary<string, string> details = null)
        {
            CheckTimeUtc = checkTimeUtc;
            ResponseTime = responseTime;
            Status = status;
            Details = details ?? new Dictionary<string, string>();
        }
    }
}