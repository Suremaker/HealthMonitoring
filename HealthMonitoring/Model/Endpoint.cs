using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Helpers;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Model
{
    public class Endpoint : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger<Endpoint>();
        private readonly IHealthMonitor _monitor;

        public Endpoint(Guid id, IHealthMonitor monitor, string address, string name, string group)
        {
            Id = id;
            _monitor = monitor;
            Address = address;
            Name = name;
            Group = group;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string MonitorType { get { return _monitor.Name; } }
        public string Group { get; private set; }
        public bool IsDisposed { get; private set; }
        public EndpointHealth Health { get; private set; }

        public Endpoint Update(string group, string name)
        {
            Group = group;
            Name = name;
            return this;
        }

        public async Task CheckHealth(CancellationToken cancellationToken, IMonitorSettings settings)
        {
            Logger.DebugFormat("Checking health of {0}...", this);
            var endpointHealth = await PerformHealthCheck(cancellationToken, settings);
            LogHealthStatus(endpointHealth);
            Health = endpointHealth;
        }

        private void LogHealthStatus(EndpointHealth endpointHealth)
        {
            switch (endpointHealth.Status)
            {
                case EndpointStatus.TimedOut:
                case EndpointStatus.Unhealthy:
                    Logger.WarnFormat("Status of {0}: Status={1}, ResponseTime={2}", this, endpointHealth.Status, endpointHealth.ResponseTime);
                    break;
                case EndpointStatus.Faulty:
                    Logger.ErrorFormat("Status of {0}: Status={1}, ResponseTime={2}, Details={3}", this, endpointHealth.Status, endpointHealth.ResponseTime, endpointHealth.PrettyFormatDetails());
                    break;
                default:
                    Logger.InfoFormat("Status of {0}: Status={1}, ResponseTime={2}", this, endpointHealth.Status, endpointHealth.ResponseTime);
                    break;
            }
        }

        private async Task<EndpointHealth> PerformHealthCheck(CancellationToken cancellationToken, IMonitorSettings settings)
        {
            var healthCheckTime = DateTime.UtcNow;
            try
            {
                var timeoutTask = ConfigureTimeoutTask(settings, cancellationToken);
                using (var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var healthTask = _monitor.CheckHealthAsync(Address, timeoutToken.Token);

                    var healthResult = await Task.WhenAny(healthTask, timeoutTask);
                    await CancelHealthTaskIfNeeded(healthTask, timeoutToken);

                    return EndpointHealth.FromResult(healthCheckTime, healthResult.Result, settings.HealthyResponseTimeLimit);
                }
            }
            catch (AggregateException e)
            {
                return EndpointHealth.FromException(healthCheckTime, e.InnerExceptions.First());
            }
            catch (Exception e)
            {
                return EndpointHealth.FromException(healthCheckTime, e);
            }
        }

        private static async Task CancelHealthTaskIfNeeded(Task<HealthInfo> healthTask, CancellationTokenSource timeoutToken)
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

        private async Task<HealthInfo> ConfigureTimeoutTask(IMonitorSettings settings, CancellationToken cancellationToken)
        {
            if (Health == null ||
                (Health.Status == EndpointStatus.Healthy || Health.Status == EndpointStatus.NotRun ||
                 Health.Status == EndpointStatus.Offline))
            {
                await Task.Delay(settings.ShortTimeOut, cancellationToken);
                return new HealthInfo(HealthStatus.TimedOut, settings.ShortTimeOut, new Dictionary<string, string> { { "message", "health check timeout" } });
            }

            await Task.Delay(settings.FailureTimeOut, cancellationToken);
            return new HealthInfo(HealthStatus.Faulty, settings.FailureTimeOut, new Dictionary<string, string> { { "message", "health check timeout" } });
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} ({2}: {3})", Group, Name, MonitorType, Address);
        }
    }
}