using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("HealthMonitoring.AcceptanceTests.Xunit.TestFramework", "HealthMonitoring.AcceptanceTests")]
namespace HealthMonitoring.AcceptanceTests.Xunit
{
    public class TestInitalization
    {
        private static Process _apiProcess;
        private static Process _monitorProcess;

        public static void Initialize()
        {
            DeleteDatabase();

            _apiProcess = Process.Start(new ProcessStartInfo("api\\HealthMonitoring.SelfHost.exe") { WindowStyle = ProcessWindowStyle.Minimized });
            _monitorProcess = Process.Start(new ProcessStartInfo("monitor\\HealthMonitoring.Monitors.SelfHost.exe") { WindowStyle = ProcessWindowStyle.Minimized });
        }

        private static void DeleteDatabase()
        {
            var dbFile = ConfigurationManager.AppSettings["DatabaseFile"];
            if (File.Exists(dbFile))
                File.Delete(dbFile);
        }

        public static void Terminate()
        {
            _apiProcess.CloseMainWindow();
            _monitorProcess.CloseMainWindow();

            if (!_apiProcess.WaitForExit(3000))
                _apiProcess.Kill();
            if (!_monitorProcess.WaitForExit(3000))
                _monitorProcess.Kill();
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
            TestInitalization.Initialize();
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
