using System;
using HealthMonitoring.Configuration;

namespace HealthMonitoring.Monitors.Core.Exchange.Entities
{
    internal class MonitorSettings : IMonitorSettings
    {
        public TimeSpan HealthCheckInterval { get; set; }
        public TimeSpan HealthyResponseTimeLimit { get; set; }
        public TimeSpan ShortTimeOut { get; set; }
        public TimeSpan FailureTimeOut { get; set; }
        public TimeSpan StatsHistoryMaxAge { get; set; }
    }
}