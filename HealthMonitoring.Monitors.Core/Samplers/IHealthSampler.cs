using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Samplers
{
    public interface IHealthSampler
    {
        Task<EndpointHealth> CheckHealthAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken);
    }
}