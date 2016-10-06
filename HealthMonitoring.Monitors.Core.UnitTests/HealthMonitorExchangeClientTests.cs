using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange.Client;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class HealthMonitorExchangeClientTests
    {
        class TestableHealthMonitorExchangeClient : HealthMonitorExchangeClient
        {
            private readonly HttpMessageHandler _handler;

            public TestableHealthMonitorExchangeClient(HttpMessageHandler handler, ITimeCoordinator timeCoordinator) : base("http://mock", timeCoordinator)
            {
                _handler = handler;
            }

            protected override HttpClient CreateClient()
            {
                return new HttpClient(_handler);
            }
        }

        class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<Tuple<Func<HttpRequestMessage, bool>, Func<HttpRequestMessage, HttpResponseMessage>>> _handlers = new List<Tuple<Func<HttpRequestMessage, bool>, Func<HttpRequestMessage, HttpResponseMessage>>>();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                foreach (var handler in _handlers)
                {
                    if (handler.Item1(request))
                        return Task.FromResult(handler.Item2(request));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            public void Setup(Func<HttpRequestMessage, bool> predicate, Func<HttpRequestMessage, HttpResponseMessage> action)
            {
                _handlers.Add(Tuple.Create(predicate, action));
            }
            public void Setup(string url, Func<HttpRequestMessage, HttpResponseMessage> action)
            {
                Setup(req => req.RequestUri.ToString().Equals(url, StringComparison.OrdinalIgnoreCase), action);
            }
        }

        [Fact]
        public async Task UploadHealthAsync_should_pass_clientCurrentTime_with_request()
        {
            var mockHttp = new MockHttpMessageHandler();
            var mockTimeCoordinator = new Mock<ITimeCoordinator>();
            var currentTimeUtc = DateTime.UtcNow;
            mockTimeCoordinator.Setup(c => c.UtcNow).Returns(currentTimeUtc);
            var client = new TestableHealthMonitorExchangeClient(mockHttp, mockTimeCoordinator.Object);

            mockHttp.Setup(
                $"http://mock/api/endpoints/health?clientCurrentTime={currentTimeUtc.ToString("u", CultureInfo.InvariantCulture)}", 
                req => new HttpResponseMessage(HttpStatusCode.OK));

            try
            {
                var update = new EndpointHealthUpdate(Guid.NewGuid(), new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Faulty));
                await client.UploadHealthAsync(new[] { update }, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Client should not throw, but it did: {e.Message}", e);
            }
        }
    }
}