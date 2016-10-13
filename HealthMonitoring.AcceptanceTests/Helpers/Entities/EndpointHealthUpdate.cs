using System;
using System.Collections.Generic;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    public class EndpointHealthUpdate
    {
        public Guid EndpointId { get; set; }
        public EndpointStatus Status { get; set; }
        public DateTime CheckTimeUtc { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public Dictionary<string, string> Details { get; set; }
    }
}
