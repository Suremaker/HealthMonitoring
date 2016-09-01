using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb5.Rabbitmq
{
    public class Nsb5Monitor : IHealthMonitor
    {
        private static readonly HealthInfo MessageTimeoutResponse = new HealthInfo(HealthStatus.Faulty, new Dictionary<string, string> { { "message", "health check timeout" } });
        private readonly IBus _bus;
        private readonly TimeSpan _messageTimeout;
        public string Name => "nsb5.rabbitmq";

        public Nsb5Monitor()
        {
            _messageTimeout = GetMessageTimeout();
            _bus = BusProvider.Create(_messageTimeout);
        }

        private TimeSpan GetMessageTimeout()
        {
            var timeout = ConfigurationManager.AppSettings["Monitor.Nsb5.Rabbitmq.MessageTimeout"];
            TimeSpan result;
            if (timeout != null && TimeSpan.TryParse(timeout, out result))
                return result;
            return TimeSpan.FromSeconds(30);
        }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            using (var wait = new ResponseWaiter(requestId, _messageTimeout))
            {
                _bus.Send(Address.Parse(address), new GetStatusRequest { RequestId = requestId });
                var response = await wait.GetResponseAsync(cancellationToken);

                return response != null ? Healthy(response) : MessageTimeoutResponse;
            }
        }

        private static HealthInfo Healthy(GetStatusResponse response)
        {
            return new HealthInfo(HealthStatus.Healthy,  response.Details);
        }
    }
}
