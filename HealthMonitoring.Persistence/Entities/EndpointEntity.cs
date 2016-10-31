using System;

namespace HealthMonitoring.Persistence.Entities
{
    internal class EndpointEntity
    {
        public Guid Id { get; set; }
        public string MonitorType { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Tags { get; set; }
        public string Password { get; set; }
        public DateTime? FirstTimeRegistered { get; set; }
        public DateTime? LastTimeRegistrationUpdated { get; set; }
    }
}