using System;
using System.Collections.Generic;

namespace HealthMonitoring.Protocols
{
    public class HealthInfo
    {
        public HealthInfo(HealthStatus status, TimeSpan responseTime) : this(status, responseTime, new Dictionary<string, string>()) { }
        public HealthInfo(HealthStatus status, TimeSpan responseTime, IReadOnlyDictionary<string, string> details)
        {
            Status = status;
            ResponseTime = responseTime;
            Details = details ?? new Dictionary<string, string>();
        }

        public HealthStatus Status { get; private set; }
        public TimeSpan ResponseTime { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }
    }
}