using System.Collections.Generic;
using HealthMonitoring.Monitors;

namespace HealthMonitoring
{
    public interface IHealthMonitorRegistry
    {
        IEnumerable<IHealthMonitor> Monitors { get; }
        IHealthMonitor FindByName(string monitorType);
    }
}