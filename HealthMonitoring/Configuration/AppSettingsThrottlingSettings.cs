using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Common.Logging;

namespace HealthMonitoring.Configuration
{
    class AppSettingsThrottlingSettings : IThrottlingSettings
    {
        private static readonly ILog Logger = LogManager.GetLogger<AppSettingsThrottlingSettings>();
        public IReadOnlyDictionary<string, int> Throttling { get; private set; }

        public AppSettingsThrottlingSettings()
        {
            Throttling = ReadConfiguration();
        }

        private static Dictionary<string, int> ReadConfiguration()
        {
            var dictionary = new Dictionary<string, int>();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("Throttling.")))
            {
                if (dictionary.ContainsKey(key))
                {
                    Logger.WarnFormat("Duplicate throtting value for key: {0}", key);
                    continue;
                }
                var stringValue = ConfigurationManager.AppSettings[key];
                int limit;
                if (!int.TryParse(stringValue, out limit))
                {
                    Logger.WarnFormat("Unable to parse throttling limit for key={0}. Value={1} is not an int.", key, stringValue);
                    continue;
                }
                var monitorType = key.Substring("Throttling.".Length);
                Logger.InfoFormat("Set throttling limit for monitor type {0} to {1}",monitorType,limit);
                dictionary.Add(monitorType, limit);
            }
            return dictionary;
        }
    }
}