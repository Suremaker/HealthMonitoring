using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.Monitoring;
using HealthMonitoring.Integration.PushClient.Registration;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Integration.PushClient
{
    public class HealthMonitorPushClient
    {
        private static readonly ILog _logger = LogManager.GetLogger<HealthMonitorPushClient>();
        private readonly IHealthMonitorClient _client;
        private readonly ITimeCoordinator _timeCoordinator;
        private EndpointDefinition _definition;
        private Func<CancellationToken, Task<HealthInfo>> _healthCheckMethod;

        protected HealthMonitorPushClient(IHealthMonitorClient client, ITimeCoordinator timeCoordinator)
        {
            _client = client;
            _timeCoordinator = timeCoordinator;
        }

        public static HealthMonitorPushClient UsingHealthMonitor(string healthMonitorUrl)
        {
            if (healthMonitorUrl == null)
                throw new ArgumentNullException(nameof(healthMonitorUrl));

            HealthMonitorClient client = null;
            if (!string.IsNullOrWhiteSpace(healthMonitorUrl))
                client = new HealthMonitorClient(healthMonitorUrl);
            else
                _logger.Warn("Health Monitor Integration would be skipped (HealthMonitor URL is empty)...");

            return new HealthMonitorPushClient(client, new DefaultTimeCoordinator());
        }

        public HealthMonitorPushClient DefineEndpoint(Action<IEndpointDefintionBuilder> definitionBuilder)
        {
            if (_definition != null)
                throw new InvalidOperationException("Endpoint already defined.");
            var builder = new EndpointDefintionBuilder();
            definitionBuilder.Invoke(builder);
            _definition = builder.Build();
            return this;
        }

        public HealthMonitorPushClient WithHealthCheckMethod(Func<CancellationToken, Task<HealthInfo>> healthCheckMethod)
        {
            _healthCheckMethod = healthCheckMethod;
            return this;
        }

        public IEndpointHealthNotifier StartHealthNotifier()
        {
            if (_definition == null)
                throw new InvalidOperationException("No endpoint definition provided");
            if (_healthCheckMethod == null)
                throw new InvalidOperationException("No endpoint health check method provided");

            if (_client == null)
                return null;

            _logger.Info("Starting Health Monitor integration...");
            return new EndpointHealthNotifier(_client, _timeCoordinator, _definition, _healthCheckMethod);
        }
    }
}