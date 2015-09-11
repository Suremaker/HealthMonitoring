using System;
using System.ComponentModel.DataAnnotations;
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
            Protocol = endpoint.Protocol;
            Group = endpoint.Group;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Protocol { get; set; }
        [Required]
        public string Group { get; set; }
    }
}