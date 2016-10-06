using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Persistence.Entities
{
    internal class EndpointStatsEntity
    {
        public long ResponseTime { get; set; }
        public EndpointStatus Status { get; set; }
        public DateTime CheckTimeUtc { get; set; }

        public EndpointStats ToDomain()
        {
            return new EndpointStats(CheckTimeUtc, Status, TimeSpan.FromTicks(ResponseTime));
        }
    }
}