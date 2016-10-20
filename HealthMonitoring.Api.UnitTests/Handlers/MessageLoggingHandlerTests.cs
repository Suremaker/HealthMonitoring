using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.SelfHost.Handlers;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Handlers
{
    public class MessageLoggingHandlerTests
    {
        private const string ResponseContent = "my content";

        class TestableMessageLoggingHandler : MessageLoggingHandler
        {
            public TestableMessageLoggingHandler(ILog logger)
                : base(logger)
            {
            }
        }

        private readonly TestableMessageLoggingHandler _handler;
        private readonly Mock<ILog> _logger;

        public MessageLoggingHandlerTests()
        {
            _logger = new Mock<ILog>();
            _handler = new TestableMessageLoggingHandler(_logger.Object);
        }

        [Theory]
        [InlineData("text/html")]
        [InlineData("text/css")]
        [InlineData("image/png")]
        [InlineData("image/x-icon")]
        [InlineData("image/svg+xml")]
        [InlineData("image/jpeg")]
        [InlineData("image/gif")]
        public async Task HandleResponse_should_log_response_without_content_of_the_response_if_content_type_is_not_allowed(string mimeType)
        {
            var response = CreateResponse(HttpStatusCode.BadRequest, mimeType);
            var correlationId = Guid.NewGuid();

            await _handler.HandleResponse(response, correlationId);

            _logger.Verify(l => l.Info($"ID: {correlationId} | status: [{(int)response.StatusCode} {response.ReasonPhrase}]"));
        }


        [Theory]
        [InlineData("text/plain")]
        [InlineData("application/json")]
        [InlineData("text/xml")]
        [InlineData("application/xml")]
        public async Task HandleResponse_should_return_response_content_if_content_type_is_allowed_and_status_code_is_400_or_higher(string mimeType)
        {
            var response = CreateResponse(HttpStatusCode.BadRequest, mimeType);

            var correlationId = Guid.NewGuid();

            await _handler.HandleResponse(response, correlationId);

            _logger.Verify(l => l.Info($"ID: {correlationId} | status: [{(int)response.StatusCode} {response.ReasonPhrase}] | content: {ResponseContent}"));
        }

        [Theory]
        [InlineData(HttpStatusCode.Continue)]
        [InlineData(HttpStatusCode.SwitchingProtocols)]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Ambiguous)]
        [InlineData(HttpStatusCode.TemporaryRedirect)]
        public async Task HandleResponse_should_log_response_without_content_if_status_code_is_100_200_or_300(HttpStatusCode statusCode)
        {
            var response = CreateResponse(statusCode, "text/plain");
            var correlationId = Guid.NewGuid();

            await _handler.HandleResponse(response, correlationId);

            _logger.Verify(l => l.Info($"ID: {correlationId} | status: [{(int)statusCode} {response.ReasonPhrase}]"));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task HandleResponse_should_log_response_with_content_if_status_code_is_400_or_higher(HttpStatusCode statusCode)
        {
            var response = CreateResponse(statusCode, "text/plain");
            var correlationId = Guid.NewGuid();

            await _handler.HandleResponse(response, correlationId);

            _logger.Verify(l => l.Info($"ID: {correlationId} | status: [{(int)statusCode} {response.ReasonPhrase}] | content: {ResponseContent}"));
        }

        [Fact]
        public async Task HandleRequest_should_log_request()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("http://localhost/abc?def")
            };

            var correlationId = await _handler.HandleRequest(request);
            _logger.Verify(l => l.Info($"ID: {request.GetCorrelationId()} | {request.Method}: [{request.RequestUri}]"));

            Assert.Equal(request.GetCorrelationId(), correlationId);
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string mimeType)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(ResponseContent)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue(mimeType)
                    }
                }
            };
        }
    }
}
