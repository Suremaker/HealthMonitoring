using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.SelfHost.Handlers
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = await HandleRequest(request);

            var response = await base.SendAsync(request, cancellationToken);

            await HandleResponse(response, correlationId);

            return response;
        }

        public abstract Task<Guid> HandleRequest(HttpRequestMessage request);
        public abstract Task HandleResponse(HttpResponseMessage response, Guid correlationId);
    }
}
