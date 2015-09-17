using System;

namespace HealthMonitoring.Persistence
{
    internal class EndpointEntity
    {
        public Guid Id { get; set; }
        public string MonitorType { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
    }
}