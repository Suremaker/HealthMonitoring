using System.Collections.Generic;

namespace HealthMonitoring.Management.Core
{
    public interface IHealthMonitorTypeRegistry
    {
        void RegisterMonitorType(string monitorType);
        IEnumerable<string> GetMonitorTypes();
    }
}