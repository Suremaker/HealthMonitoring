using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb5.Rabbitmq
{
    public class Nsb5Monitor : IHealthMonitor, IDisposable
    {
        private static readonly HealthInfo MessageTimeoutResponse = new HealthInfo(HealthStatus.Faulty, new Dictionary<string, string> { { "message", "health check timeout" } });
        private IBus _bus;
        private readonly TimeSpan _messageTimeout;
        public string Name => "nsb5.rabbitmq";

        public Nsb5Monitor()
        {
            _messageTimeout = GetMessageTimeout();
            _bus = CreateBus();
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
                SendHealthRequest(address, requestId);
                var response = await wait.GetResponseAsync(cancellationToken);

                return response != null ? Healthy(response) : MessageTimeoutResponse;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SendHealthRequest(string address, Guid requestId)
        {
            if (_bus == null)
                _bus = CreateBus();

            try
            {
                _bus.Send(Address.Parse(address), new GetStatusRequest { RequestId = requestId });
            }
            catch (ObjectDisposedException)
            {
                _bus = null;
                throw;
            }
        }

        private IBus CreateBus()
        {
            return BusProvider.Create(_messageTimeout);
        }

        private static HealthInfo Healthy(GetStatusResponse response)
        {
            return new HealthInfo(HealthStatus.Healthy, response.Details);
        }

        public void Dispose()
        {
            _bus.Dispose();
        }
    }
}
