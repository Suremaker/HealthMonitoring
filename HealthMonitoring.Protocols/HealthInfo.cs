using System;

namespace HealthMonitoring.Protocols
{
    public class HealthInfo
    {
        public HealthInfo(HealthStatus status, TimeSpan responseTime)
        {
            Status = status;
            ResponseTime = responseTime;
        }

        public HealthStatus Status { get; private set; }
        public TimeSpan ResponseTime { get; private set; }
    }
}