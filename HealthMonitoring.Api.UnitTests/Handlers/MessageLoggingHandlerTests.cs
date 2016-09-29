using System.Net.Http;
using System.Net.Http.Headers;
using HealthMonitoring.SelfHost.Handlers;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Handlers
{
    public class MessageLoggingHandlerTests
    {
        private readonly MessageLoggingHandler _handler;
        private readonly Mock<HttpResponseMessage> _response;
        private readonly Mock<HttpResponseMessage> _request;

        public MessageLoggingHandlerTests()
        {
            _handler = new MessageLoggingHandler();
            _request = new Mock<HttpResponseMessage>();
            _response = new Mock<HttpResponseMessage>();
        }

        [Theory]
        [InlineData("text/html")]
        [InlineData("text/css")]
        [InlineData("image/png")]
        [InlineData("image/x-icon")]
        [InlineData("image/svg+xml")]
        [InlineData("image/jpeg")]
        [InlineData("image/gif")]
        public async void FilterContentResponse_should_return_empty_string_if_content_type_not_allowed(string mimeType)
        {
            _response.Object.Content = new HttpMessageContent(_request.Object);
            _response.Object.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            string content = await _handler.FilterResponseContent(_response.Object);

            Assert.Equal(content, string.Empty);
        }

        [Theory]
        [InlineData("text/plain")]
        [InlineData("application/json")]
        [InlineData("text/xml")]
        [InlineData("application/xml")]
        public async void FilterContentResponse_should_return_response_content_if_content_type_is_allowed(string mimeType)
        {
            _response.Object.Content = new HttpMessageContent(_request.Object);
            _response.Object.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            string content = await _handler.FilterResponseContent(_response.Object);

            Assert.NotEqual(content, string.Empty);
            Assert.True(content.Length > 0);
        }
    }
}
