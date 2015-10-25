using System;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    internal class EndpointHealthStats
    {
        public EndpointStatus Status { get; set; }
        public DateTime CheckTimeUtc { get; set; }
        public TimeSpan ResponseTime { get; set; }
    }
}