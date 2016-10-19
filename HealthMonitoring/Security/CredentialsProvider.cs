using System;
using System.Collections.Specialized;
using System.Configuration;

namespace HealthMonitoring.Security
{
    public class CredentialsProvider : ICredentialsProvider
    {
        private const string _adminPasswordConfigKey = "AdminPassword";
        private const string _adminIdConfigKey = "AdminId";
        private const string _monitorPasswordConfigKey = "MonitorPassword";
        private const string _monitorIdConfigKey = "MonitorId";
        private const string _configurationSectionName = "accessConfiguration";

        public NameValueCollection AccessConfiguration { get; }

        public CredentialsProvider()
        {
            AccessConfiguration = (NameValueCollection)ConfigurationManager.GetSection(_configurationSectionName);
        }

        public Credentials GetAdminCredentials()
        {
            return new Credentials(
                Guid.Parse(AccessConfiguration[_adminIdConfigKey]),
                AccessConfiguration[_adminPasswordConfigKey]
            );
        }

        public Credentials GetMonitorCredentials()
        {
            return new Credentials(
                Guid.Parse(AccessConfiguration[_monitorIdConfigKey]),
                AccessConfiguration[_monitorPasswordConfigKey]
            );
        }
    }
}
