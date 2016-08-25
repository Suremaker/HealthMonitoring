using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.Persistence.Entities;

namespace HealthMonitoring.Persistence
{
    public class SqlEndpointConfigurationRepository : IEndpointConfigurationRepository
    {
        private readonly SqliteDatabase _db;

        public SqlEndpointConfigurationRepository(SqliteDatabase db)
        {
            _db = db;
        }

        public void SaveEndpoint(Endpoint endpoint)
        {
            using (var conn = _db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                if (conn.Execute("update EndpointConfig set MonitorType=@MonitorType, Address=@Address, GroupName=@Group, Name=@Name where Id=@Id", new { endpoint.Identity.MonitorType, endpoint.Identity.Address, endpoint.Metadata.Group, endpoint.Metadata.Name, endpoint.Identity.Id }, tx) == 0)
                    conn.Execute("insert into EndpointConfig (MonitorType, Address, GroupName, Name, Id) values(@MonitorType,@Address,@Group,@Name,@Id)", new { endpoint.Identity.MonitorType, endpoint.Identity.Address, endpoint.Metadata.Group, endpoint.Metadata.Name, endpoint.Identity.Id }, tx);
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

        public IEnumerable<Endpoint> LoadEndpoints()
        {
            using (var conn = _db.OpenConnection())
                return conn.Query<EndpointEntity>("select * from EndpointConfig")
                    .Select(
                        e => new Endpoint(new EndpointIdentity(e.Id, e.MonitorType, e.Address),new EndpointMetadata(e.Name, e.GroupName)))
                    .ToArray();
        }
    }
}
