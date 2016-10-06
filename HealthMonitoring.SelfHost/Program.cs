using System;
using System.Configuration;
using System.Threading;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Log4Net;
using HealthMonitoring.SelfHost.Configuration;
using Microsoft.Owin.Hosting;

namespace HealthMonitoring.SelfHost
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            LogManager.Adapter = new Log4NetLoggerFactoryAdapter(new NameValueCollection { { "configType", "FILE-WATCH" }, { "configFile", "~/log4net.config" } });
            var logger = LogManager.GetLogger<Program>();
            var baseAddress = ConfigurationManager.AppSettings["BaseUrl"];

            try
            {
                logger.Info("Starting service...");
                using (WebApp.Start<Startup>(baseAddress))
                {
                    logger.Info("Started service...");
                    Thread.Sleep(Timeout.InfiniteTimeSpan);
                }
            }
            catch (Exception e)
            {
                logger.FatalFormat("Service has terminated unexpectedly.", e);
            }
        }
    }
}
