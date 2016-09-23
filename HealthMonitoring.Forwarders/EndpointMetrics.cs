using System;

namespace HealthMonitoring.Forwarders
{
    public class EndpointMetrics
    {
        public DateTime CheckTimeUtc { get; private set; }
        public long ResponseTimeTicks { get; private set; }
        public string Status { get; private set; }

        public EndpointMetrics(DateTime checkTimeUtc, long responseTimeTicks, string status)
        {
            CheckTimeUtc = checkTimeUtc;
            ResponseTimeTicks = responseTimeTicks;
            Status = status;
        }
    }
}