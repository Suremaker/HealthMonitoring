using System.Collections.Generic;

namespace HealthMonitoring.Management.Core.Repositories
{
    public interface IHealthMonitorTypeRepository
    {
        void SaveMonitorType(string monitorType);
        IEnumerable<string> LoadMonitorTypes();
    }
}