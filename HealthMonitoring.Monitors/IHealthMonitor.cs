using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors
{
    public interface IHealthMonitor
    {
        string Name { get; }
        Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken);
    }
}