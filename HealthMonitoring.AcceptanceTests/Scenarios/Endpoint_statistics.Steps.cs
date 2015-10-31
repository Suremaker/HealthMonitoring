using System;
using System.Linq;
using System.Net;
using System.Threading;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Http;
using LightBDD;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Endpoint_statistics : FeatureFixture, IDisposable
    {
        private Guid _identifier;
        private RestClient _client;
        private MockWebEndpoint _restEndpoint;
        private IRestResponse _response;

        public Endpoint_statistics(ITestOutputHelper output)
            : base(output)
        {
        }

        public void Dispose()
        {
            if (_restEndpoint == null)
                return;
            _restEndpoint.Dispose();
            _restEndpoint = null;
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void Given_a_healthy_endpoint_working_for_some_time()
        {
            _restEndpoint = MockWebEndpointFactory.CreateNew();
            _restEndpoint.SetupStatusPlainResponse(HttpStatusCode.OK, "hello world!");
            _identifier = _client.RegisterEndpoint(MonitorTypes.Http, _restEndpoint.StatusAddress, "test", "test");
            Thread.Sleep((int) (Timeouts.HealthCheckInterval.TotalMilliseconds * 3));
        }

        private void When_client_requests_endpoint_statistics_via_url(string url)
        {
            _response = _client.Get(new RestRequest(url));
        }

        private void Then_the_response_should_containing_endpoint_statistics()
        {
            var stats = _response.DeserializeEndpointStats();
            Assert.NotEmpty(stats);
            Assert.True(stats.Length > 1, "Stats should have more than 1 record");
        }

        private void Then_statistics_should_be_ordered_ascending_by_check_time()
        {
            var stats = _response.DeserializeEndpointStats().Select(s => s.CheckTimeUtc).ToArray();
            Assert.Equal(stats.OrderBy(s => s).ToArray(), stats);
        }
    }
}