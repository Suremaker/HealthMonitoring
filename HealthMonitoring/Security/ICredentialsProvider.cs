
namespace HealthMonitoring.Security
{
    public interface ICredentialsProvider
    {
        Credentials GetAdminCredentials();
        Credentials GetMonitorCredentials();
    }
}
