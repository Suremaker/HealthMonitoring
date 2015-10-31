using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb3.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3
{
    public class Nsb3Monitor : IHealthMonitor
    {
        private static readonly HealthInfo MessageTimeoutResponse = new HealthInfo(HealthStatus.Faulty, new Dictionary<string, string> { { "message", "health check timeout" } });
        private readonly IBus _bus;
        private readonly TimeSpan _messageTimeout;
        public string Name { get { return "nsb3"; } }

        public Nsb3Monitor()
        {
            _messageTimeout = GetMessageTimeout();
            _bus = BusProvider.Create(_messageTimeout);
        }

        private TimeSpan GetMessageTimeout()
        {
            var timeout = ConfigurationManager.AppSettings["Monitor.Nsb3.MessageTimeout"];
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

    internal class ResponseWaiter : IDisposable
    {
        private readonly Guid _requestId;
        private readonly TimeSpan _timeout;
        private readonly TaskCompletionSource<GetStatusResponse> _source = new TaskCompletionSource<GetStatusResponse>();

        public ResponseWaiter(Guid requestId, TimeSpan timeout)
        {
            _requestId = requestId;
            _timeout = timeout;
            GetStatusResponseHandler.OnResponse += OnResponse;
        }

        public async Task<GetStatusResponse> GetResponseAsync(CancellationToken token)
        {
            await Task.WhenAny(_source.Task, Task.Delay(_timeout, token));
            token.ThrowIfCancellationRequested();
            return _source.Task.IsCompleted ? _source.Task.Result : null;
        }

        private void OnResponse(GetStatusResponse response)
        {
            if (response.RequestId != _requestId)
                return;
            _source.TrySetResult(response);
        }

        public void Dispose()
        {
            GetStatusResponseHandler.OnResponse -= OnResponse;
        }
    }
}
