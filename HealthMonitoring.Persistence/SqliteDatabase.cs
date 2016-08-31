using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace HealthMonitoring.Persistence
{
    public class SqliteDatabase
    {
        private static readonly string Path = ConfigurationManager.AppSettings["DatabaseFile"];
        private readonly string _connectionString = BuildConnectionString();

        public SqliteDatabase()
        {
            CreateDatabaseIfNeeded();
        }

        private static string BuildConnectionString()
        {
            return new SQLiteConnectionStringBuilder
            {
                DataSource = Path,
                Version = 3,
                Pooling = true,
                JournalMode = SQLiteJournalModeEnum.Wal,
                DefaultIsolationLevel = IsolationLevel.ReadCommitted,
                BusyTimeout = 10000,
                SyncMode = SynchronizationModes.Normal
            }.ToString();
        }

        public IDbConnection OpenConnection()
        {
            return new SQLiteConnection(_connectionString, true).OpenAndReturn();
        }

        private void CreateDatabaseIfNeeded()
        {
            using (var conn = OpenConnection())
            {
                if (!DoesTableExists(conn, "EndpointConfig"))
                    CreateEndpointConfig(conn);
                if (!DoesTableExists(conn, "EndpointStats"))
                    CreateEndpointStats(conn);
                if (!DoesTableExists(conn, "HealthMonitorTypes"))
                    CreateHealthMonitorTypesTable(conn);
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
    Name varchar(1024) not null,
    Tags varchar(4096)
)");
        }

        private void CreateHealthMonitorTypesTable(IDbConnection conn)
        {
            conn.Execute("create table HealthMonitorTypes(MonitorType varchar(256) primary key)");
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