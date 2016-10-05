using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange.Client;
using HealthMonitoring.TimeManagement;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class HealthMonitorExchangeClientTests
    {
        class TestableHealthMonitorExchangeClient : HealthMonitorExchangeClient
        {
            private readonly HttpMessageHandler _handler;

            public TestableHealthMonitorExchangeClient(HttpMessageHandler handler, ITimeCoordinator timeCoordinator) : base("http://mock/", timeCoordinator)
            {
                _handler = handler;
            }

            protected override HttpClient CreateClient()
            {
                return new HttpClient(_handler);
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

            mockHttp.Expect($"*/api/endpoints/health?clientCurrentTime={currentTimeUtc.ToString("u", CultureInfo.InvariantCulture)}")
                .Respond(HttpStatusCode.OK);

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