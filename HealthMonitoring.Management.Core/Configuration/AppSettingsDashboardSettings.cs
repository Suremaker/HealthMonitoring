using System.Configuration;
using Common.Logging;
using HealthMonitoring.Configuration;

namespace HealthMonitoring.Management.Core.Configuration
{
    public class AppSettingsDashboardSettings : IDashboardSettings
    {
        private static readonly ILog Logger = LogManager.GetLogger<AppSettingsDashboardSettings>();
        public string Title { get; }
        public string Version { get; }

        public AppSettingsDashboardSettings()
        {
            Title = GetValueOrDefault("Dashboard.Title", "Dashboard");
            Version = GetType().Assembly.GetName().Version.ToString(4);
        }

        private string GetValueOrDefault(string name, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[name];
            if (value != null)
            {
                Logger.InfoFormat("Using setting: {0} = {1}", name, value);
                return value;
            }
            Logger.InfoFormat("Using default setting: {0} = {1}", name, defaultValue);
            return defaultValue;
        }
    }
}