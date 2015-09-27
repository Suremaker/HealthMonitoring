using System;
using System.Configuration;
using System.Threading;
using Common.Logging;
using Common.Logging.Configuration;
using HealthMonitoring.SelfHost.Configuration;
using Microsoft.Owin.Hosting;

namespace HealthMonitoring.SelfHost
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger<Program>();
        public static void Main(params string[] args)
        {
            LogManager.Adapter = new Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter(new NameValueCollection { { "configType", "FILE-WATCH" }, { "configFile", "~/log4net.config" } });
            var baseAddress = ConfigurationManager.AppSettings["BaseUrl"];

            try
            {
                Logger.Info("Starting service...");
                using (WebApp.Start<Startup>(baseAddress))
                {
                    Logger.Info("Started service...");
                    Thread.Sleep(Timeout.InfiniteTimeSpan);
                }
            }
            catch (Exception e)
            {
                Logger.FatalFormat("Service has terminated unexpectedly:\n{0}", e);
            }
        }
    }
}
