using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Client.Models;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.Registration;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Integration.PushClient.Monitoring
{
    internal class EndpointHealthNotifier : IEndpointHealthNotifier
    {
        private static readonly ILog _logger = LogManager.GetLogger<EndpointHealthNotifier>();
        private readonly IHealthMonitorClient _client;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly EndpointDefinition _definition;
        private readonly Func<CancellationToken, Task<HealthInfo>> _healthCheckMethod;
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancelationTokenSource;
        private readonly CachedValue<TimeSpan> _healthCheckInterval;
        private Guid? _endpointId;

        public EndpointHealthNotifier(IHealthMonitorClient client, ITimeCoordinator timeCoordinator, EndpointDefinition definition, Func<CancellationToken, Task<HealthInfo>> healthCheckMethod)
        {
            _client = client;
            _timeCoordinator = timeCoordinator;
            _definition = definition;
            _healthCheckMethod = healthCheckMethod;
            _cancelationTokenSource = new CancellationTokenSource();
            _healthCheckInterval = new CachedValue<TimeSpan>(TimeSpan.FromMinutes(10), GetHealthCheckIntervalAsync);
            _thread = new Thread(HealthLoop) { IsBackground = true, Name = "Health Check loop" };
            _thread.Start();
        }

        private void HealthLoop()
        {
            try
            {
                HealthLoopAsync().Wait();
            }
            catch (Exception e)
            {
                _logger.Error("Endpoint health notification stopped unexpectedly.", e);
            }
        }

        private async Task HealthLoopAsync()
        {
            while (!_cancelationTokenSource.IsCancellationRequested)
            {
                await Task.WhenAll(PerformHealthCheckAsync(), SynchronizeCheckIntervalAsync());
            }
        }

        private async Task SynchronizeCheckIntervalAsync()
        {
            try
            {
                await _timeCoordinator.Delay(await _healthCheckInterval.GetValueAsync(), _cancelationTokenSource.Token);
            }
            catch (OperationCanceledException) when (_cancelationTokenSource.IsCancellationRequested)
            {
            }
            catch (Exception e)
            {
                _logger.Error("Health check loop synchrnonisation failed.", e);
            }
        }

        private async Task PerformHealthCheckAsync()
        {
            HealthInfo healthInfo;
            var checkTimeUtc = DateTime.UtcNow;
            var watch = Stopwatch.StartNew();
            try
            {
                healthInfo = await _healthCheckMethod.Invoke(_cancelationTokenSource.Token);
            }
            catch (OperationCanceledException) when (_cancelationTokenSource.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                _logger.Error("Unable to collect health information", e);
                healthInfo = new HealthInfo(HealthStatus.Faulty, new Dictionary<string, string> { { "reason", "Unable to collect health information" }, { "exception", e.ToString() } });
            }
            await SendHealthUpdateAsync(checkTimeUtc, watch.Elapsed, healthInfo);
        }

        private async Task SendHealthUpdateAsync(DateTime checkTimeUtc, TimeSpan checkTime, HealthInfo healthInfo)
        {
            var endpointId = _endpointId ?? (_endpointId = await RegisterEndpointAsync()).Value;

            try
            {
                await _client.SendHealthUpdateAsync(endpointId, new HealthUpdate(checkTimeUtc, checkTime, healthInfo), _cancelationTokenSource.Token);
            }
            catch (EndpointNotFoundException)
            {
                _endpointId = null;
            }
        }

        public void Dispose()
        {
            if (_cancelationTokenSource.IsCancellationRequested)
                return;
            _cancelationTokenSource.Cancel();
            _thread.Join();
            _cancelationTokenSource.Dispose();
        }

        private Task<Guid> RegisterEndpointAsync()
        {
            return _client.RegisterEndpointAsync(_definition, _cancelationTokenSource.Token);
        }

        private Task<TimeSpan> GetHealthCheckIntervalAsync()
        {
            return _client.GetHealthCheckIntervalAsync(_cancelationTokenSource.Token);
        }
    }
}