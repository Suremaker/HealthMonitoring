using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.SelfHost.Models
{
    public class PublicEndpointIdentity
    {
        public Guid Id { get; private set; }
        public string MonitorType { get; private set; }
        public string Address { get; private set; }

        public PublicEndpointIdentity(EndpointIdentity identity)
        {
            Id = identity.Id;
            MonitorType = identity.MonitorType;
            Address = identity.Address;
        }
    }
}
