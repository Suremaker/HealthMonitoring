using System.Collections.Generic;

namespace HealthMonitoring.Management.Core.Registers
{
    public interface IHealthMonitorTypeRegistry
    {
        void RegisterMonitorType(string monitorType);
        IEnumerable<string> GetMonitorTypes();
    }
}