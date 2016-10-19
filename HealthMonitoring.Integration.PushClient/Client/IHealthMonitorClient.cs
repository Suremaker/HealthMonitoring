using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient.Client.Models;
using HealthMonitoring.Integration.PushClient.Registration;

namespace HealthMonitoring.Integration.PushClient.Client
{
    public interface IHealthMonitorClient
    {
        Task<Guid> RegisterEndpointAsync(EndpointDefinition definition, CancellationToken cancellationToken);
        Task<TimeSpan> GetHealthCheckIntervalAsync(CancellationToken cancellationToken);
        Task SendHealthUpdateAsync(Guid endpointId, string authenticationToken, HealthUpdate update, CancellationToken cancellationToken);
    }
}