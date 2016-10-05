using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Model;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Management.Core
{
    public class EndpointUpdateFrequencyGuard : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger<EndpointUpdateFrequencyGuard>();
        private readonly IEndpointRegistry _endpointRegistry;
        private readonly IContinuousTaskExecutor<Endpoint> _taskExecutor;
        private readonly IMonitorSettings _monitorSettings;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly TimeSpan _maxEndpointDelay;

        public EndpointUpdateFrequencyGuard(IEndpointRegistry endpointRegistry, IContinuousTaskExecutor<Endpoint> taskExecutor, IMonitorSettings monitorSettings, ITimeCoordinator timeCoordinator)
        {
            _endpointRegistry = endpointRegistry;
            _taskExecutor = taskExecutor;
            _monitorSettings = monitorSettings;
            _timeCoordinator = timeCoordinator;

            _maxEndpointDelay = GetEndpointMaxUpdateDelay();

            _endpointRegistry.EndpointAdded += AddEndpoint;
            foreach (var endpoint in _endpointRegistry.Endpoints)
                AddEndpoint(endpoint);
        }

        private void AddEndpoint(Endpoint endpoint)
        {
            _taskExecutor.TryRegisterTaskFor(endpoint, MonitorEndpointUpdatesAsync);
        }

        private async Task MonitorEndpointUpdatesAsync(Endpoint endpoint, CancellationToken cancellationToken)
        {
            while (!endpoint.IsDisposed)
            {
                if (WasEndpointUpdateMissed(endpoint))
                    ReportEndpointTimeout(endpoint);

                await _timeCoordinator.Delay(GetEndpointUpdateCheckDelay(endpoint), cancellationToken);
            }
        }

        private void ReportEndpointTimeout(Endpoint endpoint)
        {
            var health = new EndpointHealth(_timeCoordinator.UtcNow, TimeSpan.Zero, EndpointStatus.TimedOut,
                new Dictionary<string, string>
                {
                    {"reason", "Endpoint health was not updated within specified period of time."}
                });

            _endpointRegistry.UpdateHealth(endpoint.Identity.Id, health);
            _logger.Warn($"Endpoint Id={endpoint.Identity.Id} health was not updated within specified period of time.");
        }

        private TimeSpan GetEndpointUpdateCheckDelay(Endpoint endpoint)
        {
            var delayDuration = GetEndpointLastCheckUtc(endpoint) + _maxEndpointDelay - _timeCoordinator.UtcNow;
            return delayDuration > TimeSpan.Zero ? delayDuration : TimeSpan.Zero;
        }

        private DateTime GetEndpointLastCheckUtc(Endpoint endpoint)
        {
            return endpoint.Health?.CheckTimeUtc ?? endpoint.LastModifiedTimeUtc.UtcDateTime;
        }

        private bool WasEndpointUpdateMissed(Endpoint endpoint)
        {
            return _timeCoordinator.UtcNow - GetEndpointLastCheckUtc(endpoint) > _maxEndpointDelay;
        }

        private TimeSpan GetEndpointMaxUpdateDelay()
        {
            return _monitorSettings.HealthCheckInterval + _monitorSettings.HealthUpdateInactivityTimeLimit;
        }

        public void Dispose()
        {
            _endpointRegistry.EndpointAdded -= AddEndpoint;
            _taskExecutor.Dispose();
        }
    }
}
