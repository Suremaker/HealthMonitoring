using System;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    class PublicEndpointIdentity
    {
        public Guid Id { get; set; }
        public string MonitorType { get; set; }
        public string Address { get; set; }
    }
}
