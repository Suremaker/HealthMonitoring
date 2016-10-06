using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Monitors.Core
{
    public class EndpointMonitor : IDisposable
    {
        private const int MaxErrorDelayInSeconds = 120;
        private static readonly ILog Logger = LogManager.GetLogger<EndpointMonitor>();
        private readonly IMonitorSettings _settings;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly IMonitorableEndpointRegistry _monitorableEndpointRegistry;
        private readonly IHealthSampler _sampler;
        private readonly Random _randomizer = new Random();
        private readonly IContinuousTaskExecutor<MonitorableEndpoint> _executor;

        public EndpointMonitor(IMonitorableEndpointRegistry monitorableEndpointRegistry, IHealthSampler sampler, IMonitorSettings settings, ITimeCoordinator timeCoordinator, IContinuousTaskExecutor<MonitorableEndpoint> executor)
        {
            _monitorableEndpointRegistry = monitorableEndpointRegistry;
            _sampler = sampler;
            _settings = settings;
            _timeCoordinator = timeCoordinator;
            _executor = executor;

            _monitorableEndpointRegistry.NewEndpointAdded += HandleNewEndpoint;

            foreach (var endpoint in _monitorableEndpointRegistry.Endpoints)
                _executor.TryRegisterTaskFor(endpoint, MonitorEndpointAsync);
        }

        private async Task MonitorEndpointAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken)
        {
            int errorCounter = 0;

            await _timeCoordinator.Delay(GetRandomizedDelay(), cancellationToken);
            while (!cancellationToken.IsCancellationRequested && !endpoint.IsDisposed)
            {
                try
                {
                    var delay = _timeCoordinator.Delay(_settings.HealthCheckInterval, cancellationToken);
                    await Task.WhenAll(endpoint.CheckHealth(_sampler, cancellationToken), delay);
                    errorCounter = 0;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (AggregateException e) when (cancellationToken.IsCancellationRequested && e.Flatten().InnerExceptions.All(ex => ex is OperationCanceledException))
                {
                }
                catch (Exception e)
                {
                    errorCounter = Math.Min(errorCounter + 1, MaxErrorDelayInSeconds);
                    Logger.ErrorFormat("Monitoring error ({0} occurrence): {1}", errorCounter, e.ToString());
                    await _timeCoordinator.Delay(TimeSpan.FromSeconds(errorCounter), cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            _monitorableEndpointRegistry.NewEndpointAdded -= HandleNewEndpoint;
            _executor.Dispose();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TimeSpan GetRandomizedDelay()
        {
            var delay = _randomizer.NextDouble() * _settings.HealthCheckInterval.TotalMilliseconds;
            return TimeSpan.FromMilliseconds(delay);
        }

        private void HandleNewEndpoint(MonitorableEndpoint endpoint)
        {
            _executor.TryRegisterTaskFor(endpoint, MonitorEndpointAsync);
        }

    }
}
