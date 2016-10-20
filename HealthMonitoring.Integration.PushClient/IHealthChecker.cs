using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient
{
    public interface IHealthChecker
    {
        Task<EndpointHealth> CheckHealthAsync(CancellationToken cancellationToken);
    }
}