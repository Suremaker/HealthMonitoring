using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace HealthMonitoring.Persistence
{
    public class SqliteDatabase
    {
        private static readonly string _path = ConfigurationManager.AppSettings["DatabaseFile"];

        public SqliteDatabase()
        {
            CreateDatabaseIfNeeded();
        }

        public IDbConnection OpenConnection()
        {
            return new SQLiteConnection("Data Source=" + _path + ";Version=3;PRAGMA journal_mode=WAL;").OpenAndReturn();
        }

        private void CreateDatabaseIfNeeded()
        {
            using (var conn = OpenConnection())
            {
                if (!DoesTableExists(conn, "EndpointConfig"))
                    CreateEndpointConfig(conn);
                if (!DoesTableExists(conn, "EndpointStats"))
                    CreateEndpointStats(conn);
            }
        }

        private bool DoesTableExists(IDbConnection conn, string tableName)
        {
            return conn
                .Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;", new { tableName })
                .Any();
        }

        private static void CreateEndpointConfig(IDbConnection conn)
        {
            conn.Execute(@"
create table EndpointConfig (
    Id uniqueidentifier primary key, 
    MonitorType varchar(100) not null, 
    Address varchar(2048) not null, 
    GroupName varchar(1024) not null, 
    Name varchar(1024) not null
)");
        }

        private void CreateEndpointStats(IDbConnection conn)
        {
            conn.Execute(@"
create table EndpointStats (
    Id integer primary key,
    EndpointId uniqueidentifier not null,
    CheckTimeUtc datetime not null,
    ResponseTime integer not null,
    Status integer not null
);

create index EndpointStats_EndpointId_idx on EndpointStats(EndpointId);
create index EndpointStats_CheckTimeUtc_idx on EndpointStats(CheckTimeUtc);
");
        }
    }
}