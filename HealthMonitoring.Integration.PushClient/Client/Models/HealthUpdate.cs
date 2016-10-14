using System;
using System.Collections.Generic;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Integration.PushClient.Client.Models
{
    public class HealthUpdate
    {
        public DateTime CheckTimeUtc { get; private set; }
        public HealthStatus Status { get; private set; }
        public TimeSpan ResponseTime { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }

        public HealthUpdate(DateTime checkTimeUtc, TimeSpan checkTime, HealthInfo info)
        {
            CheckTimeUtc = checkTimeUtc;
            Status = info.Status;
            ResponseTime = checkTime;
            Details = info.Details;
        }
    }
}