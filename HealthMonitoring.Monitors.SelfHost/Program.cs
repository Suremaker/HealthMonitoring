using System;
using System.Configuration;
using System.Threading;
using Autofac;
using Common.Logging;
using Common.Logging.Configuration;
using HealthMonitoring.Configuration;
using HealthMonitoring.Hosting;
using HealthMonitoring.Monitors.Core;
using HealthMonitoring.Monitors.Core.Exchange;
using HealthMonitoring.Monitors.Core.Exchange.Client;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.SelfHost.Configuration;

namespace HealthMonitoring.Monitors.SelfHost
{
    public class Program
    {
        private static ILog _logger;

        public static int Main()
        {
            LogManager.Adapter = new Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter(new NameValueCollection { { "configType", "FILE-WATCH" }, { "configFile", "~/log4net.config" } });
            _logger = LogManager.GetLogger<Program>();
            try
            {
                _logger.Info("Starting service...");
                using (StartHost())
                {
                    _logger.Info("Started service...");
                    Thread.Sleep(Timeout.InfiniteTimeSpan);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception e)
            {
                _logger.FatalFormat("Service has terminated unexpectedly.", e);
                return 1;
            }
            return 0;
        }

        private static IContainer StartHost()
        {
            var exchangeClient = new HealthMonitorExchangeClient(ConfigurationManager.AppSettings["HealthMonitoringUrl"]);
            var settings = LoadSettings(exchangeClient);

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(HealthMonitorRegistry).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(exchangeClient).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(settings.MonitorSettings).AsImplementedInterfaces();
            builder.RegisterInstance(settings.ThrottlingSettings).AsImplementedInterfaces();
            builder.RegisterInstance(AppSettingsDataExchangeConfigProvider.ReadConfiguration());

            builder.Register(c => new ThrottlingSampler(c.Resolve<HealthSampler>(), c.Resolve<IThrottlingSettings>())).AsImplementedInterfaces();

            builder.RegisterInstance<IHealthMonitorRegistry>(new HealthMonitorRegistry(PluginDiscovery<IHealthMonitor>.DiscoverAllInCurrentFolder("*.Monitors.*.dll")));
            builder.RegisterType<EndpointMonitor>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MonitorDataExchange>().AsSelf().AsImplementedInterfaces().SingleInstance();

            var container = builder.Build();
            container.Resolve<EndpointMonitor>();
            return container;
        }

        private static HealthMonitorSettings LoadSettings(IHealthMonitorExchangeClient exchangeClient)
        {
            int attempts = 0;
            int totalAttempts = 10;
            while (true)
            {
                try
                {
                    return exchangeClient.LoadSettingsAsync(CancellationToken.None).Result;
                }
                catch (Exception e)
                {
                    _logger.Warn($"Unable to read configuration: {e.Message}");
                    if (++attempts == totalAttempts)
                        throw;
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
