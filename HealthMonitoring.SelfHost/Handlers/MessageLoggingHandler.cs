using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;

namespace HealthMonitoring.SelfHost.Handlers
{
    public class MessageLoggingHandler : MessageHandler 
    {
        private static readonly ILog Logger = LogManager.GetLogger<MessageLoggingHandler>();

        public override async Task<Guid> HandleRequest(HttpRequestMessage request)
        {
            Guid correlationId = request.GetCorrelationId();

            await Task.Run(() => Logger.InfoFormat("ID: {0} | {1}: [{2}]",
                correlationId,
                request.Method, 
                request.RequestUri));

            return await Task.FromResult(correlationId);
        }

        public override async Task HandleResponse(HttpResponseMessage response, Guid correlationId)
        {
            await Task.Run(() => Logger.InfoFormat("ID: {0} | status: [{1} {2}]",
                correlationId,
                (int)response.StatusCode,
                response.ReasonPhrase));
        }
    }
}
