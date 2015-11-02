using HealthMonitoring.Configuration;

namespace HealthMonitoring.SelfHost.Entities
{
    public class Config
    {
        public Config(IMonitorSettings monitor, IDashboardSettings dashboard)
        {
            Monitor = monitor;
            Dashboard = dashboard;
        }

        public IMonitorSettings Monitor { get; private set; }
        public IDashboardSettings Dashboard { get; private set; }
    }
}