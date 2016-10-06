using System;
using System.Configuration;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public static class Timeouts
    {
        public static readonly TimeSpan Default = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan HealthCheckInterval = TimeSpan.Parse(ConfigurationManager.AppSettings["Monitor.HealthCheckInterval"]);
    }
}