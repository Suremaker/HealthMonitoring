using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Exchange
{
    public class EndpointHealthUpdate
    {
        public EndpointHealthUpdate(Guid endpointId, EndpointHealth health)
        {
            EndpointId = endpointId;
            Health = health;
        }

        public Guid EndpointId { get; }
        public EndpointHealth Health { get; }
    }
}