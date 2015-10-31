using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Persistence.Entities;

namespace HealthMonitoring.Persistence
{
    public class SqlEndpointConfigurationStore : IEndpointConfigurationStore
    {
        private readonly SqliteDatabase _db;

        public SqlEndpointConfigurationStore(SqliteDatabase db)
        {
            _db = db;
        }

        public void SaveEndpoint(Endpoint endpoint)
        {
            using (var conn = _db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                if (conn.Execute("update EndpointConfig set MonitorType=@MonitorType, Address=@Address, GroupName=@Group, Name=@Name where Id=@Id", new { endpoint.MonitorType, endpoint.Address, endpoint.Group, endpoint.Name, endpoint.Id }, tx) == 0)
                    conn.Execute("insert into EndpointConfig (MonitorType, Address, GroupName, Name, Id) values(@MonitorType,@Address,@Group,@Name,@Id)", new { endpoint.MonitorType, endpoint.Address, endpoint.Group, endpoint.Name, endpoint.Id }, tx);
                tx.Commit();
            }
        }

        public void DeleteEndpoint(Guid endpointId)
        {
            using (var conn = _db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                conn.Execute("delete from EndpointConfig where Id=@endpointId", new { endpointId }, tx);
                tx.Commit();
            }
        }

        public IEnumerable<Endpoint> LoadEndpoints(IHealthMonitorRegistry monitorRegistry)
        {
            using (var conn = _db.OpenConnection())
                return conn.Query<EndpointEntity>("select * from EndpointConfig")
                    .Select(
                        e => new Endpoint(e.Id, monitorRegistry.FindByName(e.MonitorType), e.Address, e.Name, e.GroupName))
                    .ToArray();
        }
    }
}
