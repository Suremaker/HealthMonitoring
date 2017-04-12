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
using ITimeCoordinator = HealthMonitoring.Integration.PushClient.Helpers.ITimeCoordinator;

namespace HealthMonitoring.Integration.PushClient.Monitoring
{
    internal class EndpointHealthNotifier : IEndpointHealthNotifier
    {
        private static readonly ILog _logger = LogManager.GetLogger<EndpointHealthNotifier>();
        private readonly IHealthMonitorClient _client;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly EndpointDefinition _definition;
        private readonly IHealthChecker _healthChecker;
        private readonly IBackOffStategy _backOffStategy;
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancelationTokenSource;
        private readonly CachedValue<TimeSpan> _healthCheckInterval;
        private Guid? _endpointId;
        private static readonly TimeSpan HealthCheckIntervalCacheDuration = TimeSpan.FromMinutes(10);
        private TimeSpan? _retryInterval;

        public EndpointHealthNotifier(
            IHealthMonitorClient client, 
            ITimeCoordinator timeCoordinator, 
            EndpointDefinition definition, 
            IHealthChecker healthChecker, 
            IBackOffStategy backOffStategy)
        {
            _client = client;
            _timeCoordinator = timeCoordinator;
            _definition = definition;
            _healthChecker = healthChecker;
            _backOffStategy = backOffStategy;
            _cancelationTokenSource = new CancellationTokenSource();
            _healthCheckInterval = new CachedValue<TimeSpan>(HealthCheckIntervalCacheDuration, GetHealthCheckIntervalAsync);
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
            EndpointHealth endpointHealth;
            var checkTimeUtc = DateTime.UtcNow;
            var watch = Stopwatch.StartNew();
            try
            {
                endpointHealth = await _healthChecker.CheckHealthAsync(_cancelationTokenSource.Token);
                if (endpointHealth == null)
                    throw new InvalidOperationException("Health information not provided");
            }
            catch (OperationCanceledException) when (_cancelationTokenSource.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                _logger.Error("Unable to collect health information", e);
                endpointHealth = new EndpointHealth(
                    HealthStatus.Faulty, 
                    new Dictionary<string, string> { { "reason", "Unable to collect health information" }, { "exception", e.ToString() } });
            }

            await EnsureSendHealthUpdateAsync(new HealthUpdate(checkTimeUtc, watch.Elapsed, endpointHealth));
        }

        private async Task EnsureSendHealthUpdateAsync(HealthUpdate update)
        {
            while (!_cancelationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await SendHealthUpdateAsync(update);
                    _retryInterval = null;
                    return;
                }
                catch (Exception e) when (!_cancelationTokenSource.IsCancellationRequested)
                {
                    BackOffPlan backOffPlan = await _backOffStategy.GetCurrent(_retryInterval, _cancelationTokenSource.Token);

                    if (backOffPlan.ShouldLog)
                    {
                        _logger.Error("Unable to send health update", e);
                    }

                    _retryInterval = backOffPlan.RetryInterval;

                    if (_retryInterval.HasValue)
                    {
                        await SafeDelay(_retryInterval.Value);
                    }
                }
            }
        }
        
        private async Task SafeDelay(TimeSpan delay)
        {
            try
            {
                await _timeCoordinator.Delay(delay, _cancelationTokenSource.Token);
            }
            catch { }
        }

        private async Task SendHealthUpdateAsync(HealthUpdate update)
        {
            var endpointId = _endpointId ?? (_endpointId = await RegisterEndpointAsync()).Value;

            try
            {
                await _client.SendHealthUpdateAsync(endpointId, _definition.Password, update, _cancelationTokenSource.Token);
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