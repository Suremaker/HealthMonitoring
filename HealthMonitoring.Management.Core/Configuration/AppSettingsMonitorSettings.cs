using System;
using System.Configuration;
using Common.Logging;
using HealthMonitoring.Configuration;

namespace HealthMonitoring.Management.Core.Configuration
{
    public class AppSettingsMonitorSettings : IMonitorSettings
    {
        private static readonly ILog Logger = LogManager.GetLogger<AppSettingsMonitorSettings>();
        public TimeSpan HealthCheckInterval { get; }
        public TimeSpan HealthyResponseTimeLimit { get; }
        public TimeSpan ShortTimeOut { get; }
        public TimeSpan FailureTimeOut { get; }
        public TimeSpan StatsHistoryMaxAge { get; }

        public AppSettingsMonitorSettings()
        {
            HealthCheckInterval = GetTimeSpanOrDefault("Monitor.HealthCheckInterval", TimeSpan.FromSeconds(5));
            HealthyResponseTimeLimit = GetTimeSpanOrDefault("Monitor.HealthyResponseTimeLimit", TimeSpan.FromSeconds(3));
            ShortTimeOut = GetTimeSpanOrDefault("Monitor.ShortTimeOut", TimeSpan.FromSeconds(4));
            FailureTimeOut = GetTimeSpanOrDefault("Monitor.FailureTimeOut", TimeSpan.FromSeconds(20));
            StatsHistoryMaxAge = GetTimeSpanOrDefault("Monitor.StatsHistoryMaxAge", TimeSpan.FromDays(7));
        }

        private TimeSpan GetTimeSpanOrDefault(string name, TimeSpan defaultValue)
        {
            var value = ConfigurationManager.AppSettings[name];
            TimeSpan result;
            if (value != null && TimeSpan.TryParse(value, out result))
            {
                Logger.InfoFormat("Using setting: {0} = {1}", name, value);
                return result;
            }
            Logger.InfoFormat("Using default setting: {0} = {1}", name, defaultValue);
            return defaultValue;
        }
    }
}