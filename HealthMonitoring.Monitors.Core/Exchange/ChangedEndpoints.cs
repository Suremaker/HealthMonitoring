using System;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Exchange
{
    public class ChangedEndpoints
    {
        public DateTimeOffset CheckTime { get; set; }
        public EndpointIdentity[] NewEndpoints { get; set; }
        public EndpointIdentity[] DeletedEndpoints { get; set; }
    }
}