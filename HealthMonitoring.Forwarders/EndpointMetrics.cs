using System;

namespace HealthMonitoring.Forwarders
{
    public class EndpointMetrics
    {
        public DateTime CheckTimeUtc { get; private set; }
        public long ResponseTimeMilliseconds { get; private set; }
        public string Status { get; private set; }

        public EndpointMetrics(DateTime checkTimeUtc, long responseTimeMilliseconds, string status)
        {
            CheckTimeUtc = checkTimeUtc;
            ResponseTimeMilliseconds = responseTimeMilliseconds;
            Status = status;
        }
    }
}