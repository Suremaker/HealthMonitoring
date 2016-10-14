using System;
using System.Collections.Generic;

namespace HealthMonitoring.Integration.PushClient.Client.Models
{
    public class HealthUpdate
    {
        public DateTime CheckTimeUtc { get; }
        public HealthStatus Status { get; }
        public TimeSpan ResponseTime { get; }
        public IReadOnlyDictionary<string, string> Details { get; }

        public HealthUpdate(DateTime checkTimeUtc, TimeSpan responseTime, EndpointHealth info)
        {
            CheckTimeUtc = checkTimeUtc;
            Status = info.Status;
            ResponseTime = responseTime;
            Details = info.Details;
        }
    }
}