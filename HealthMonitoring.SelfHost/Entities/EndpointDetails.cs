using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HealthMonitoring.Model;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointDetails
    {
        public EndpointDetails() { }
        public EndpointDetails(Endpoint endpoint)
        {
            Id = endpoint.Id;
            Name = endpoint.Name;
            Address = endpoint.Address;
            MonitorType = endpoint.MonitorType;
            Group = endpoint.Group;
            if (endpoint.Health != null)
            {
                Status = endpoint.Health.Status;
                LastCheckUtc = endpoint.Health.CheckTimeUtc;
                LastResponseTime = endpoint.Health.ResponseTime;
                Details = endpoint.Health.Details.ToDictionary(p => p.Key, p => p.Value);
            }
            else
            {
                Details = new Dictionary<string, string>();
            }
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string MonitorType { get; set; }
        [Required]
        public string Group { get; set; }
        [Required]
        public EndpointStatus Status { get; set; }
        public DateTime? LastCheckUtc { get; set; }
        public TimeSpan? LastResponseTime { get; set; }
        [Required]
        public IDictionary<string, string> Details { get; set; }
    }
}