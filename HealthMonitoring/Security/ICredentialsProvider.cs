
namespace HealthMonitoring.Security
{
    public interface ICredentialsProvider
    {
        Credentials GetAdminMonitorCredentials();
        Credentials GetPullMonitorCredentials();
    }
}
