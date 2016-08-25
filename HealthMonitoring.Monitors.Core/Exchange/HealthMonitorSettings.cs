using HealthMonitoring.Configuration;

namespace HealthMonitoring.Monitors.Core.Exchange
{
    public class HealthMonitorSettings
    {
        public HealthMonitorSettings(IMonitorSettings monitorSettings, IThrottlingSettings throttlingSettings)
        {
            MonitorSettings = monitorSettings;
            ThrottlingSettings = throttlingSettings;
        }

        public IMonitorSettings MonitorSettings { get; }
        public IThrottlingSettings ThrottlingSettings { get; }
    }
}