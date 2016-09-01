using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HealthMonitoring.Model;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointHealthUpdate
    {
        [Required]
        public Guid EndpointId { get; set; }
        [Required]
        public EndpointStatus Status { get; set; }
        [Required]
        public DateTime CheckTimeUtc { get; set; }
        [Required]
        public TimeSpan ResponseTime { get; set; }
        [Required]
        public Dictionary<string, string> Details { get; set; }

        public EndpointHealth ToEndpointHealth()
        {
            return new EndpointHealth(CheckTimeUtc, ResponseTime, Status, Details);
        }
    }
}