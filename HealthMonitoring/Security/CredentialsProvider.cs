using System;
using System.Collections.Specialized;
using System.Configuration;

namespace HealthMonitoring.Security
{
    public class CredentialsProvider : ICredentialsProvider
    {
        private const string _adminTokenConfigKey = "AdminPrivateToken";
        private const string _adminIdConfigKey = "AdminId";
        private const string _monitorTokenConfigKey = "MonitorPrivateToken";
        private const string _monitorIdConfigKey = "MonitorId";
        private const string _configurationSectionName = "accessConfiguration";

        public NameValueCollection AccessConfiguration { get; }

        public CredentialsProvider()
        {
            AccessConfiguration = (NameValueCollection)ConfigurationManager.GetSection(_configurationSectionName);
        }

        public Credentials GetAdminMonitorCredentials()
        {
            return new Credentials(
                Guid.Parse(AccessConfiguration[_adminIdConfigKey]),
                AccessConfiguration[_adminTokenConfigKey]
            );
        }

        public Credentials GetPullMonitorCredentials()
        {
            return new Credentials(
                Guid.Parse(AccessConfiguration[_monitorIdConfigKey]),
                AccessConfiguration[_monitorTokenConfigKey]
            );
        }
    }
}
