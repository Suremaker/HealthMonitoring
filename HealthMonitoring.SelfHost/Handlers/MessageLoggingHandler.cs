using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using System.Linq;

namespace HealthMonitoring.SelfHost.Handlers
{
    public class MessageLoggingHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<MessageLoggingHandler>();
        private readonly string[] _allowedMediaTypes = {"text/plain", "application/json", "text/xml", "application/xml"};

        public override async Task<Guid> HandleRequest(HttpRequestMessage request)
        {
            var correlationId = request.GetCorrelationId();

            await Task.Run(() => Logger.InfoFormat("ID: {0} | {1}: [{2}]",
                correlationId,
                request.Method,
                request.RequestUri));

            return await Task.FromResult(correlationId);
        }

        public override async Task HandleResponse(HttpResponseMessage response, Guid correlationId)
        {
            string baseInfo = $"ID: {correlationId} | status: [{(int)response.StatusCode} {response.ReasonPhrase}]";

            if (response.IsSuccessStatusCode)
            {
                await Task.Run(() => Logger.Info(baseInfo));
            }
            else
            {
                var requestContent = await response.RequestMessage.Content.ReadAsStringAsync();
                var responseContent = await FilterResponseContentAsync(response);
                await Task.Run(() => Logger.ErrorFormat("{0} | requestContent: [{1}] | responseContent: [{2}]", baseInfo, requestContent, responseContent));
            }
        }

        public async Task<string> FilterResponseContentAsync(HttpResponseMessage response)
        {
            var content = string.Empty;

            if ((response.Content != null) &&
                _allowedMediaTypes.Contains(response.Content.Headers.ContentType.MediaType))
                content = await response.Content.ReadAsStringAsync();

            return await Task.FromResult(content);
        }
    }
}
