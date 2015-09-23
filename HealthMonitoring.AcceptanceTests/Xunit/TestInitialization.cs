using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using HealthMonitoring.SelfHost;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("HealthMonitoring.AcceptanceTests.Xunit.TestFramework", "HealthMonitoring.AcceptanceTests")]
namespace HealthMonitoring.AcceptanceTests.Xunit
{
    public class TestInitalization
    {
        private static Thread _thread;

        public static void Initialize()
        {
            DisableSqlLiteErrorPrinting();
            DeleteDatabase();

            _thread = new Thread(() => Program.Main());
            _thread.Start();
        }

        private static void DisableSqlLiteErrorPrinting()
        {
            Console.SetError(Console.Out);
        }

        private static void DeleteDatabase()
        {
            var dbFile = ConfigurationManager.AppSettings["DatabaseFile"];
            if (File.Exists(dbFile))
                File.Delete(dbFile);
        }

        public static void Terminate()
        {
            _thread.Interrupt();
            _thread.Join();
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
