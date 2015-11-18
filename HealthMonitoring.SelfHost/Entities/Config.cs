using System.Collections.Generic;
using HealthMonitoring.Configuration;

namespace HealthMonitoring.SelfHost.Entities
{
    public class Config
    {
        public Config(IMonitorSettings monitor, IDashboardSettings dashboard, IThrottlingSettings throttlingSettings)
        {
            Monitor = monitor;
            Dashboard = dashboard;
            Throttling = throttlingSettings.Throttling;
        }

        public IReadOnlyDictionary<string, int> Throttling { get; private set; }
        public IMonitorSettings Monitor { get; private set; }
        public IDashboardSettings Dashboard { get; private set; }
    }
}