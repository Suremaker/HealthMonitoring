using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
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

        public static void Initialize(string assemblyPath)
        {
            DeleteDatabase();

            _monitor = StartAssembly(assemblyPath, "monitor\\HealthMonitoring.Monitors.SelfHost.exe");
            _api = StartAssembly(assemblyPath, "api\\HealthMonitoring.SelfHost.exe");
            EnsureProcessesAlive();
        }

        private static Tuple<Thread, AppDomain> StartAssembly(string assemblyPath, string exeRelativePath)
        {
            var exePath = Path.GetDirectoryName(assemblyPath) + "\\" + exeRelativePath;

            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(exePath),
                ConfigurationFile = Path.GetFileName(exePath) + ".config"
            };
            var domain = AppDomain.CreateDomain(exeRelativePath, AppDomain.CurrentDomain.Evidence, setup);
            var thread = new Thread(() => ExecuteAssembly(exePath, domain)) { IsBackground = true };
            thread.Start();
            return Tuple.Create(thread, domain);
        }

        private static int ExecuteAssembly(string exePath, AppDomain domain)
        {
            return domain.ExecuteAssembly(exePath);
        }

        private static void EnsureProcessesAlive()
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            if (_api.Item1.IsAlive && _monitor.Item1.IsAlive)
                return;

            Terminate();
            throw new InvalidOperationException("HealthMonitor processes failed to start");
        }

        private static void DeleteDatabase()
        {
            var dbFile = ConfigurationManager.AppSettings["DatabaseFile"];
            if (File.Exists(dbFile))
                File.Delete(dbFile);
        }

        public static void Terminate()
        {
            KillAppDomain(_api);
            KillAppDomain(_monitor);
        }

        private static void KillAppDomain(Tuple<Thread, AppDomain> process)
        {
            try
            {
                AppDomain.Unload(process.Item2);
                process.Item1.Join();
            }
            catch { }
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
