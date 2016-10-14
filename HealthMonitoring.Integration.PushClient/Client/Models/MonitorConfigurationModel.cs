using System;

namespace HealthMonitoring.Integration.PushClient.Client.Models
{
    internal class MonitorConfigurationModel
    {
        public TimeSpan HealthCheckInterval { get; set; }
    }
}