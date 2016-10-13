using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.Persistence.Entities;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Persistence
{
    public class SqlEndpointConfigurationRepository : IEndpointConfigurationRepository
    {
        private readonly MySqlDatabase _db;
        private readonly ITimeCoordinator _timeCoordinator;

        public SqlEndpointConfigurationRepository(MySqlDatabase db, ITimeCoordinator timeCoordinator)
        {
            _db = db;
            _timeCoordinator = timeCoordinator;
        }

        public void SaveEndpoint(Endpoint endpoint)
        {
            using (var conn = _db.OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                var tags = endpoint.Metadata.Tags.ToDbString();
                
                conn.Execute(
                    $"replace into EndpointConfig(MonitorType, Address, GroupName, Name, Id, PrivateToken{(tags != null ? ", Tags" : "")}) values(@MonitorType,@Address,@Group,@Name,@Id, @PrivateToken{(tags != null ? ",@Tags" : "")})",
                    new
                    {
                        endpoint.Identity.MonitorType,
                        endpoint.Identity.Address,
                        endpoint.Metadata.Group,
                        endpoint.Metadata.Name,
                        endpoint.Identity.Id,
                        endpoint.PrivateToken,
                        tags
                    }, tx);

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
                    .Select(e => new Endpoint(_timeCoordinator, new EndpointIdentity(e.Id, e.MonitorType, e.Address), new EndpointMetadata(e.Name, e.GroupName, e.Tags.FromDbString())))
                    .ToArray();
        }
    }
}
