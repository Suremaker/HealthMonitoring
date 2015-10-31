using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Model
{
    public interface IHealthSampler
    {
        Task<EndpointHealth> CheckHealth(Endpoint endpoint, CancellationToken cancellationToken);
    }
}