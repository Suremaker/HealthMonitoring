using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;

namespace HealthMonitoring.SelfHost.Handlers
{
    public class MessageLoggingHandler : MessageHandler 
    {
        private static readonly ILog Logger = LogManager.GetLogger<MessageLoggingHandler>();

        private readonly string[] _allowedMediaTypes = { "text/plain", "application/json", "text/xml", "application/xml" };

        public override async Task<Guid> HandleRequest(HttpRequestMessage request)
        {
            var content = await request.Content.ReadAsStringAsync();
            Guid correlationId = request.GetCorrelationId();

            await Task.Run(() => Logger.InfoFormat("ID: {0} | {1}: [{2}] | content: {3}",
                correlationId,
                request.Method, 
                request.RequestUri, 
                content));

            return await Task.FromResult(correlationId);
        }

        public override async Task HandleResponse(HttpResponseMessage response, Guid correlationId)
        {
            string content = await FilterResponseContent(response);

            await Task.Run(() => Logger.InfoFormat("ID: {0} | status: [{1} {2}] | content: {3}",
                correlationId,
                (int)response.StatusCode,
                response.ReasonPhrase,
                content));
        }

        public async Task<string> FilterResponseContent(HttpResponseMessage response)
        {
            var content = string.Empty;

            if (response.Content != null &&
                _allowedMediaTypes.Contains(response.Content.Headers.ContentType.MediaType))
            {
                content = await response.Content.ReadAsStringAsync();
            }

            return await Task.FromResult(content);
        }
    }
}
