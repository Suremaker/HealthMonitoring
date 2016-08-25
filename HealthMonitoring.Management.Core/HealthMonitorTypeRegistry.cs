using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.Management.Core.Repositories;

namespace HealthMonitoring.Management.Core
{
    public class HealthMonitorTypeRegistry : IHealthMonitorTypeRegistry
    {
        private readonly IHealthMonitorTypeRepository _repository;
        private readonly ConcurrentDictionary<string, string> _monitorTypes;

        public HealthMonitorTypeRegistry(IHealthMonitorTypeRepository repository)
        {
            _repository = repository;
            _monitorTypes = new ConcurrentDictionary<string, string>(_repository.LoadMonitorTypes().ToDictionary(t => t, t => t));
        }

        public void RegisterMonitorType(string monitorType)
        {
            _monitorTypes.AddOrUpdate(monitorType, monitorType, (key, val) => monitorType);
            _repository.SaveMonitorType(monitorType);
        }

        public IEnumerable<string> GetMonitorTypes()
        {
            return _monitorTypes.Keys;
        }
    }
}