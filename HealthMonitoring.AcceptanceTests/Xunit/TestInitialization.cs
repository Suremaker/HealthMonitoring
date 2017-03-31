using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using HealthMonitoring.AcceptanceTests.Helpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("HealthMonitoring.AcceptanceTests.Xunit.TestFramework", "HealthMonitoring.AcceptanceTests")]
namespace HealthMonitoring.AcceptanceTests.Xunit
{
    public class TestInitalization
    {
        private static Tuple<Thread, AppDomain> _api;
        private static Tuple<Thread, AppDomain> _monitor;
        private static Tuple<Thread, AppDomain> _nsb6Monitor;

        public static void Initialize(string assemblyPath)
        {
            AppDomainExecutor.Initialize(assemblyPath);
            DeleteDatabase();

            _monitor = AppDomainExecutor.StartAssembly("monitor\\HealthMonitoring.Monitors.SelfHost.exe");
            _nsb6Monitor = AppDomainExecutor.StartAssembly("monitornsb6\\HealthMonitoring.Monitors.SelfHost.exe");
            _api = AppDomainExecutor.StartAssembly("api\\HealthMonitoring.SelfHost.exe");
            EnsureProcessesAlive();
        }

        private static void EnsureProcessesAlive()
        {
            Wait.Until(
                Timeouts.Default,
                () => ClientHelper.Build().Get(new RestRequest("/api/monitors")),
                resp =>
                {
                    if (resp.StatusCode != HttpStatusCode.OK) return false;
                    var monitorTypes = JsonConvert.DeserializeObject<string[]>(resp.Content);
                    return monitorTypes.Contains("nsb6.rabbitmq") && monitorTypes.Contains("nsb5.rabbitmq");
                },
                "Services did not initialized properly");

            if (_api.Item1.IsAlive && _monitor.Item1.IsAlive && _nsb6Monitor.Item1.IsAlive)
                return;

            Terminate();
            throw new InvalidOperationException("HealthMonitor processes failed to start");
        }

        private static void DeleteDatabase()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["HealthMonitoring"].ConnectionString;
            var databaseName = ConfigurationManager.AppSettings["DatabaseName"];
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"DROP DATABASE IF EXISTS {databaseName}";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        public static void Terminate()
        {
            AppDomainExecutor.KillAppDomain(_api);
            AppDomainExecutor.KillAppDomain(_monitor);
            AppDomainExecutor.KillAppDomain(_nsb6Monitor);
        }
    }

    public class TestFramework : XunitTestFramework
    {
        public TestFramework(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new ApplicationTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }

    public class ApplicationTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        public ApplicationTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            TestInitalization.Initialize(TestAssembly.Assembly.AssemblyPath);
            try
            {
                using (XunitTestAssemblyRunner testAssemblyRunner = new XunitTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                {
                    RunSummary runSummary = await testAssemblyRunner.RunAsync();
                }
            }
            finally
            {
                TestInitalization.Terminate();
            }
        }
    }
}
