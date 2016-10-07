using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using System.Linq;

namespace HealthMonitoring.SelfHost.Handlers
{
    public class MessageLoggingHandler : MessageHandler
    {
        private readonly string[] _allowedMediaTypes = { "text/plain", "application/json", "text/xml", "application/xml" };
        private readonly ILog _logger;

        public MessageLoggingHandler() : this(LogManager.GetLogger<MessageLoggingHandler>())
        {
        }

        protected MessageLoggingHandler(ILog logger)
        {
            _logger = logger;
        }

        public override Task<Guid> HandleRequest(HttpRequestMessage request)
        {
            var correlationId = request.GetCorrelationId();

            _logger.Info($"ID: {correlationId} | {request.Method}: [{request.RequestUri}]");

            return Task.FromResult(correlationId);
        }

        public override async Task HandleResponse(HttpResponseMessage response, Guid correlationId)
        {
            string content = null;

            if ((int)response.StatusCode >= 400)
                content = await FilterResponseContent(response);

            if (content != null)
                _logger.Info($"ID: {correlationId} | status: [{(int)response.StatusCode} {response.ReasonPhrase}] | content: {content}");
            else
                _logger.Info($"ID: {correlationId} | status: [{(int)response.StatusCode} {response.ReasonPhrase}]");
        }

        private async Task<string> FilterResponseContent(HttpResponseMessage response)
        {
            if (response.Content != null && _allowedMediaTypes.Contains(response.Content.Headers.ContentType.MediaType))
                return await response.Content.ReadAsStringAsync();
            return null;
        }
    }
}
