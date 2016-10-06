using System.Collections.Generic;

namespace HealthMonitoring.Monitors.Core.Registers
{
    public interface IHealthMonitorRegistry
    {
        IEnumerable<IHealthMonitor> Monitors { get; }
        IEnumerable<string> MonitorTypes { get; }
        IHealthMonitor FindByName(string monitorType);
    }
}