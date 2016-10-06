using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core
{
    public interface IEndpointHealthUpdateListener
    {
        void UpdateHealth(Guid endpointId, EndpointHealth endpointHealth);
    }
}