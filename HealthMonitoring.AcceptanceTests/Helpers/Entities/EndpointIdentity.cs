using System;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    internal class EndpointIdentity
    {
        public string Address { get; set; }
        public string MonitorType { get; set; }
        public Guid Id { get; set; }
    }
}