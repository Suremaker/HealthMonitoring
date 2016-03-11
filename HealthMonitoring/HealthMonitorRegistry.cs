using System.Collections.Generic;
using Common.Logging;
using HealthMonitoring.Monitors;

namespace HealthMonitoring
{
    public class HealthMonitorRegistry : IHealthMonitorRegistry
    {
        private static readonly ILog Logger = LogManager.GetLogger<HealthMonitorRegistry>();
        private readonly IDictionary<string, IHealthMonitor> _monitors = new Dictionary<string, IHealthMonitor>();

        public HealthMonitorRegistry(IEnumerable<IHealthMonitor> monitors)
        {
            foreach (var monitor in monitors)
            {
                if (!_monitors.ContainsKey(monitor.Name))
                    _monitors.Add(monitor.Name, monitor);
                else
                    Logger.WarnFormat("Monitor with name {0} already exists. The {1} is not going to be registered", monitor.Name, monitor);
            }
        }

        public IEnumerable<IHealthMonitor> Monitors => _monitors.Values;

        public IHealthMonitor FindByName(string monitorType)
        {
            IHealthMonitor monitor;
            return _monitors.TryGetValue(monitorType, out monitor) ? monitor : null;
        }
    }
}