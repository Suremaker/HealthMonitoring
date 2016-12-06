using System;
using System.Configuration;
using Common.Logging;
using HealthMonitoring.Model;
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
                GetTimeSpanOrDefault(nameof(DataExchangeConfig.EndpointChangeQueryInterval), TimeSpan.FromMinutes(1)),
                GetStringOrDefault(nameof(DataExchangeConfig.MonitorTag), EndpointMetadata.DefaultMonitorTag));
        }

        private static T GetSettingOrDefault<T>(string name, Func<string, T> parser, T defaultValue)
        {
            var value = GetSetting(name);
            if (value == null)
            {
                Logger.Info($"Using default setting: {name} = {defaultValue}");
                return defaultValue;
            }

            Logger.Info($"Using setting: {name} = {value}");
            try
            {
                return parser(value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to parse setting: {name} = {value}", e);
            }
        }

        private static int GetIntOrDefault(string name, int defaultValue) => GetSettingOrDefault(name, int.Parse, defaultValue);
        private static TimeSpan GetTimeSpanOrDefault(string name, TimeSpan defaultValue) => GetSettingOrDefault(name, TimeSpan.Parse, defaultValue);
        private static string GetStringOrDefault(string name, string defaultValue) => GetSettingOrDefault(name, value => value, defaultValue);

        private static string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[nameof(DataExchangeConfig) + "." + name];
        }
    }
}