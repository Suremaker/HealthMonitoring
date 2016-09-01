using System;
using System.Configuration;
using Common.Logging;
using HealthMonitoring.Monitors.Core.Exchange;

namespace HealthMonitoring.Monitors.SelfHost.Configuration
{
    static class AppSettingsDataExchangeConfigProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppSettingsDataExchangeConfigProvider));

        public static DataExchangeConfig ReadConfiguration()
        {
            return new DataExchangeConfig(
                GetIntOrDefault(nameof(DataExchangeConfig.OutgoingQueueMaxCapacity), 1024),
                GetIntOrDefault(nameof(DataExchangeConfig.ExchangeOutBucketSize), 64),
                GetTimeSpanOrDefault(nameof(DataExchangeConfig.UploadRetryInterval), TimeSpan.FromSeconds(5)),
                GetTimeSpanOrDefault(nameof(DataExchangeConfig.EndpointChangeQueryInterval), TimeSpan.FromMinutes(1)));
        }

        private static int GetIntOrDefault(string name, int defaultValue)
        {
            var value = GetSetting(name);
            int result;
            if (value != null && int.TryParse(value, out result))
            {
                Logger.InfoFormat("Using setting: {0} = {1}", name, value);
                return result;
            }
            Logger.InfoFormat("Using default setting: {0} = {1}", name, defaultValue);
            return defaultValue;
        }

        private static TimeSpan GetTimeSpanOrDefault(string name, TimeSpan defaultValue)
        {
            var value = GetSetting(name);
            TimeSpan result;
            if (value != null && TimeSpan.TryParse(value, out result))
            {
                Logger.InfoFormat("Using setting: {0} = {1}", name, value);
                return result;
            }
            Logger.InfoFormat("Using default setting: {0} = {1}", name, defaultValue);
            return defaultValue;
        }

        private static string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[nameof(DataExchangeConfig) + "." + name];
        }
    }
}