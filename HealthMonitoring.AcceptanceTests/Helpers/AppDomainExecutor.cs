using System;
using System.IO;
using System.Threading;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public class AppDomainExecutor
    {
        private static string _assemblyPath;

        public static Tuple<Thread, AppDomain> StartAssembly(string exeRelativePath)
        {
            if (_assemblyPath == null)
                throw new InvalidOperationException("AppDomainExecutor not initialized");

            var exePath = Path.GetDirectoryName(_assemblyPath) + "\\" + exeRelativePath;

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

        public static void KillAppDomain(Tuple<Thread, AppDomain> process)
        {
            try
            {
                AppDomain.Unload(process.Item2);
                process.Item1.Join();
            }
            catch { }
        }

        public static void Initialize(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }
    }
}