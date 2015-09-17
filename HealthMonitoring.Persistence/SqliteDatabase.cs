using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;

namespace HealthMonitoring.Persistence
{
    public class SqliteDatabase
    {
        private static readonly string _path = Environment.CurrentDirectory + "\\monitoring.db";

        public SqliteDatabase()
        {
            if (!File.Exists(_path))
                CreateDatabase();
        }

        public IDbConnection OpenConnection()
        {
            return new SQLiteConnection("Data Source=" + _path).OpenAndReturn();
        }

        private void CreateDatabase()
        {
            using (var conn = OpenConnection())
            {
                conn.Execute("create table EndpointConfig (Id uniqueidentifier primary key, MonitorType varchar(100) not null, Address varchar(2048) not null, GroupName varchar(1024) not null, Name varchar(1024) not null)");
            }
        }
    }
}