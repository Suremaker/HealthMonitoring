using System;
using System.Configuration;

namespace HealthMonitoring.Security
{
    public class CredentialsProvider : ICredentialsProvider
    {
        private string _adminTokenConfigKey = "AdminMonitorPrivateToken";
        private string _adminIdConfigKey = "AdminMonitorId";
        private string _pullMonitorTokenConfigKey = "PullMonitorPrivateToken";
        private string _pullMonitorIdConfigKey = "PullMonitorId";

        public Credentials GetAdminMonitorCredentials()
        {
            return new Credentials(
                Guid.Parse(ConfigurationManager.AppSettings[_adminIdConfigKey]),
                ConfigurationManager.AppSettings[_adminTokenConfigKey]
            );
        }

        public Credentials GetPullMonitorCredentials()
        {
            return new Credentials(
                Guid.Parse(ConfigurationManager.AppSettings[_pullMonitorIdConfigKey]),
                ConfigurationManager.AppSettings[_pullMonitorTokenConfigKey]
            );
        }
    }
}
