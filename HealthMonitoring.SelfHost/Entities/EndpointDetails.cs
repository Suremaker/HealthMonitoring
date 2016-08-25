using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Model;

namespace HealthMonitoring.SelfHost.Entities
{
    public class EndpointDetails
    {
        public EndpointDetails() { }
        private EndpointDetails(Endpoint endpoint)
        {
            Id = endpoint.Identity.Id;
            Address = endpoint.Identity.Address;
            MonitorType = endpoint.Identity.MonitorType;
            Name = endpoint.Metadata.Name;
            Group = endpoint.Metadata.Group;
            var health = endpoint.Health;
            LastModifiedTime = endpoint.LastModifiedTime;
            if (health != null)
            {
                Status = health.Status;
                LastCheckUtc = health.CheckTimeUtc;
                LastResponseTime = health.ResponseTime;
                Details = health.Details.ToDictionary(p => p.Key, p => p.Value);
            }
            else
            {
                Details = new Dictionary<string, string>();
                Status = EndpointStatus.NotRun;
            }
        }

        public static EndpointDetails FromDomain(Endpoint endpoint)
        {
            return new EndpointDetails(endpoint);
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTimeOffset LastModifiedTime { get; set; }
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