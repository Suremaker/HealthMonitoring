using System;

namespace HealthMonitoring.Model
{
    public class EndpointStats
    {
        public EndpointStats(DateTime checkTimeUtc, EndpointStatus status, TimeSpan responseTime)
        {
            CheckTimeUtc = checkTimeUtc;
            Status = status;
            ResponseTime = responseTime;
        }

        public TimeSpan ResponseTime { get; private set; }
        public EndpointStatus Status { get; private set; }
        public DateTime CheckTimeUtc { get; private set; }
    }
}