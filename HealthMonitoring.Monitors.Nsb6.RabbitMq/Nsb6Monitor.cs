using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb6.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb6.RabbitMq
{
    public class Nsb6Monitor : IHealthMonitor, IDisposable
    {
        private static readonly HealthInfo MessageTimeoutResponse = new HealthInfo(HealthStatus.Faulty, new Dictionary<string, string> { { "message", "health check timeout" } });
        private readonly TimeSpan _messageTimeout;
        public string Name => "nsb6.rabbitmq";
        private IEndpointInstance _endpoint;

        public Nsb6Monitor()
        {
            _messageTimeout = GetMessageTimeout();
            var endpointConfiguration = EndpointConfigurationProvider.BuildEndpointConfiguration(_messageTimeout);
            StartAsync(endpointConfiguration).GetAwaiter().GetResult();
        }

        private TimeSpan GetMessageTimeout()
        {
            var timeout = ConfigurationManager.AppSettings["Monitor.Nsb6.RabbitMq.MessageTimeout"];
            TimeSpan result;
            if (timeout != null && TimeSpan.TryParse(timeout, out result))
                return result;
            return TimeSpan.FromSeconds(30);
        }

        private async Task StartAsync(EndpointConfiguration buildEndpointConfiguration)
        {
            _endpoint = await Endpoint.Start(buildEndpointConfiguration).ConfigureAwait(false);
        }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            using (var wait = new ResponseWaiter(requestId, _messageTimeout))
            {
                await _endpoint.Send(address, new GetStatusRequest { RequestId = requestId });
                var response = await wait.GetResponseAsync(cancellationToken);

                return response != null ? Healthy(response) : MessageTimeoutResponse;
            }
        }

        private static HealthInfo Healthy(GetStatusResponse response)
        {
            return new HealthInfo(HealthStatus.Healthy, response.Details);
        }

        private async Task StopAsync()
        {
            await _endpoint.Stop().ConfigureAwait(false);
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
