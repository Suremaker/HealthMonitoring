using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;

namespace HealthMonitoring.Monitors.Core.Exchange.Client
{
    public interface IHealthMonitorExchangeClient
    {
        Task RegisterMonitorsAsync(IEnumerable<string> monitorTypes, CancellationToken token);
        Task<EndpointIdentity[]> GetEndpointIdentitiesAsync(CancellationToken token);
        Task UploadHealthAsync(EndpointHealthUpdate[] updates, CancellationToken token);

        Task<HealthMonitorSettings> LoadSettingsAsync(CancellationToken token);
    }
}