using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Protocols
{
    public interface IHealthCheckProtocol
    {
        string Name { get; }
        Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken);
    }
}