using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using HealthMonitoring.Model;
using HealthMonitoring.Persistence.Entities;

namespace HealthMonitoring.Persistence
{
    public class EndpointStatsRepository : IEndpointStatsRepository
    {
        private readonly SqliteDatabase _db;

        public EndpointStatsRepository(SqliteDatabase db)
        {
            _db = db;
        }

        public void InsertEndpointStatistics(Guid endpointId, EndpointHealth stats)
        {
            using (var conn = _db.OpenConnection())
            {
                conn.Execute(
                    "insert into EndpointStats (EndpointId, CheckTimeUtc, ResponseTime, Status) values (@endpointId, @checkTimeUtc, @responseTime, @status)",
                    new { endpointId, stats.CheckTimeUtc, responseTime = stats.ResponseTime.Ticks, stats.Status });
            }
        }

        public IEnumerable<EndpointStats> GetStatistics(Guid id, int limitDays)
        {
            using (var conn = _db.OpenConnection())
            {
                return conn
                    .Query<EndpointStatsEntity>("select CheckTimeUtc, ResponseTime, Status from EndpointStats where endpointId=@id and checkTimeUtc > @dateLimit order by checkTimeUtc", new { id, dateLimit = DateTime.UtcNow.AddDays(-limitDays) })
                    .Select(e => e.ToDomain());
            }
        }

        public void DeleteStatistics(Guid id)
        {
            using (var conn = _db.OpenConnection())
                conn.Execute("delete from EndpointStats where endpointId=@id", new {id});
        }

        public void DeleteStatisticsOlderThan(DateTime date)
        {
            using (var conn = _db.OpenConnection())
                conn.Execute("delete from EndpointStats where CheckTimeUtc < @date", new { date });
        }
    }
}