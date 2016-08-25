using System.Collections.Generic;
using System.Linq;
using Dapper;
using HealthMonitoring.Management.Core.Repositories;

namespace HealthMonitoring.Persistence
{
    public class HealthMonitorTypeRepository : IHealthMonitorTypeRepository
    {
        private readonly SqliteDatabase _db;

        public HealthMonitorTypeRepository(SqliteDatabase db)
        {
            _db = db;
        }

        public void SaveMonitorType(string monitorType)
        {
            using (var conn = _db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                conn.Execute("insert or replace into HealthMonitorTypes(MonitorType) values(@monitorType)",
                    new { monitorType });
                tx.Commit();
            }
        }

        public IEnumerable<string> LoadMonitorTypes()
        {
            using (var conn = _db.OpenConnection())
                return conn.Query<string>("select MonitorType from HealthMonitorTypes").ToArray();
        }
    }
}