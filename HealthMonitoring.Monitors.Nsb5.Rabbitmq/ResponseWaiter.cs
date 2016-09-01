using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb5.Messages;

namespace HealthMonitoring.Monitors.Nsb5.Rabbitmq
{
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