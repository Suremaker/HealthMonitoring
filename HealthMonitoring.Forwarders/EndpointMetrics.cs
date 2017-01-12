using System;

namespace HealthMonitoring.Forwarders
{
    public class EndpointMetrics
    {
        public DateTime CheckTimeUtc { get; private set; }
        public TimeSpan ResponseTime { get; private set; }
        public string Status { get; private set; }

        public EndpointMetrics(DateTime checkTimeUtc, TimeSpan responseTime, string status)
        {
            CheckTimeUtc = checkTimeUtc;
            ResponseTime = responseTime;
            Status = status;
        }
    }
}