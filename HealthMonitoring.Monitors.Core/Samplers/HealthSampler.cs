using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Helpers;

namespace HealthMonitoring.Monitors.Core.Samplers
{
    public class HealthSampler : IHealthSampler
    {
        private static readonly ILog Logger = LogManager.GetLogger<HealthSampler>();
        private readonly IMonitorSettings _settings;
        private readonly IEndpointHealthUpdateListener _healthUpdateListener;
        private static readonly Dictionary<string, string> TimeoutDetails = new Dictionary<string, string> { { "message", "health check timeout" } };

        public HealthSampler(IMonitorSettings settings, IEndpointHealthUpdateListener healthUpdateListener)
        {
            _settings = settings;
            _healthUpdateListener = healthUpdateListener;
        }

        public async Task<EndpointHealth> CheckHealthAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken)
        {
            Logger.DebugFormat("Checking health of {0}...", endpoint);
            var endpointHealth = await PerformHealthCheckAsync(cancellationToken, endpoint);
            LogHealthStatus(endpoint, endpointHealth);
            _healthUpdateListener.UpdateHealth(endpoint.Identity.Id, endpointHealth);
            return endpointHealth;
        }

        private async Task<EndpointHealth> PerformHealthCheckAsync(CancellationToken cancellationToken, MonitorableEndpoint endpoint)
        {
            var checkTimeUtc = DateTime.UtcNow;
            var timer = new Stopwatch();
            try
            {
                using (var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timer.Start();
                    var timeoutTask = ConfigureTimeoutTaskAsync(endpoint, cancellationToken);
                    var healthTask = endpoint.Monitor.CheckHealthAsync(endpoint.Identity.Address, timeoutToken.Token);
                    var healthResult = await Task.WhenAny(healthTask, timeoutTask);
                    timer.Stop();

                    await CancelHealthTaskIfNeededAsync(healthTask, timeoutToken);

                    return FromResult(checkTimeUtc, timer.Elapsed, healthResult.Result);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.First() is OperationCanceledException)
                    throw;
                return FromException(checkTimeUtc, timer.Elapsed, e.InnerExceptions.First());
            }
            catch (Exception e)
            {
                return FromException(checkTimeUtc, timer.Elapsed, e);
            }
        }

        private EndpointHealth FromException(DateTime checkTimeUtc, TimeSpan responseTime, Exception exception)
        {
            var details = new Dictionary<string, string>
            {
                {"reason", exception.Message},
                {"exception", exception.ToString()}
            };
            return new EndpointHealth(checkTimeUtc, responseTime, EndpointStatus.Faulty, details);
        }

        private EndpointHealth FromResult(DateTime checkTimeUtc, TimeSpan responseTime, HealthInfo result)
        {
            return new EndpointHealth(checkTimeUtc, responseTime, GetStatus(result.Status, responseTime), result.Details);
        }

        private EndpointStatus GetStatus(HealthStatus status, TimeSpan responseTime)
        {
            if (status == HealthStatus.Healthy && responseTime > _settings.HealthyResponseTimeLimit)
                return EndpointStatus.Unhealthy;
            return (EndpointStatus)status;
        }

        private static async Task CancelHealthTaskIfNeededAsync(Task<HealthInfo> healthTask, CancellationTokenSource timeoutToken)
        {
            if (healthTask.IsCompleted)
                return;

            timeoutToken.Cancel();
            try
            {
                await healthTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task<HealthInfo> ConfigureTimeoutTaskAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken)
        {
            if (RequiresShortTimeout(endpoint))
            {
                await Task.Delay(_settings.ShortTimeOut, cancellationToken);
                return new HealthInfo(HealthStatus.TimedOut, TimeoutDetails);
            }

            await Task.Delay(_settings.FailureTimeOut, cancellationToken);
            return new HealthInfo(HealthStatus.Faulty, TimeoutDetails);
        }

        private static void LogHealthStatus(MonitorableEndpoint endpoint, EndpointHealth endpointHealth)
        {
            switch (endpointHealth.Status)
            {
                case EndpointStatus.TimedOut:
                case EndpointStatus.Unhealthy:
                    Logger.WarnFormat("Status of {0}: Status={1}, ResponseTime={2}", endpoint, endpointHealth.Status, endpointHealth.ResponseTime);
                    break;
                case EndpointStatus.Faulty:
                    Logger.ErrorFormat("Status of {0}: Status={1}, ResponseTime={2}, Details={3}", endpoint, endpointHealth.Status, endpointHealth.ResponseTime, endpointHealth.PrettyFormatDetails());
                    break;
                default:
                    Logger.InfoFormat("Status of {0}: Status={1}, ResponseTime={2}", endpoint, endpointHealth.Status, endpointHealth.ResponseTime);
                    break;
            }
        }

        private static bool RequiresShortTimeout(MonitorableEndpoint endpoint)
        {
            return endpoint.Health == null ||
                   (endpoint.Health.Status == EndpointStatus.Healthy || endpoint.Health.Status == EndpointStatus.NotRun ||
                    endpoint.Health.Status == EndpointStatus.Offline || endpoint.Health.Status == EndpointStatus.NotExists);
        }
    }
}