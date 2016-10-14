using System;
using System.ComponentModel.DataAnnotations;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointHealthUpdate: HealthUpdate
    {
        [Required]
        public Guid EndpointId { get; set; }
    }
}