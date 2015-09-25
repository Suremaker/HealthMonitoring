using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb3.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3
{
    public class Nsb3Monitor : IHealthMonitor
    {
        private readonly IBus _bus;
        public string Name { get { return "nsb3"; } }

        public Nsb3Monitor()
        {
            _bus = BusProvider.Create();
        }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            using (var wait = new ResponseWaiter(requestId))
            {
                var watch = Stopwatch.StartNew();
                _bus.Send(Address.Parse(address), new GetStatusRequest { RequestId = requestId });
                var response = await wait.GetResponseAsync(cancellationToken);
                watch.Stop();

                return response != null ? Healthy(watch, response) : Faulty(watch);
            }
        }

        private static HealthInfo Healthy(Stopwatch watch, GetStatusResponse response)
        {
            return new HealthInfo(HealthStatus.Healthy, watch.Elapsed, response.Details);
        }

        private static HealthInfo Faulty(Stopwatch watch)
        {
            return new HealthInfo(HealthStatus.Faulty, watch.Elapsed, new Dictionary<string, string> { { "message", "health check timeout" } });
        }
    }

    internal class ResponseWaiter : IDisposable
    {
        private readonly Guid _requestId;
        private readonly TaskCompletionSource<GetStatusResponse> _source = new TaskCompletionSource<GetStatusResponse>();

        public ResponseWaiter(Guid requestId)
        {
            _requestId = requestId;
            GetStatusResponseHandler.OnResponse += OnResponse;
        }

        public async Task<GetStatusResponse> GetResponseAsync(CancellationToken token)
        {
            await Task.WhenAny(_source.Task, Task.Delay(4000, token));
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
