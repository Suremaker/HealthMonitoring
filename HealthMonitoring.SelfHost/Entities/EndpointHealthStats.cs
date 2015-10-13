using System;
using System.ComponentModel.DataAnnotations;
using HealthMonitoring.Model;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointHealthStats
    {
        [Required]
        public EndpointStatus Status { get; set; }
        [Required]
        public DateTime CheckTimeUtc { get; set; }
        [Required]
        public TimeSpan ResponseTime { get; set; }

        public static EndpointHealthStats FromDomain(EndpointStats stats)
        {
            return new EndpointHealthStats
            {
                Status = stats.Status,
                CheckTimeUtc = stats.CheckTimeUtc,
                ResponseTime = stats.ResponseTime
            };
        }
    }
}