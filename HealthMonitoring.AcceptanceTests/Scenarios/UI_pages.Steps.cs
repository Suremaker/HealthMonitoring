using System;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Http;
using LightBDD;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class UI_pages :  FeatureFixture,IDisposable
    {
        private RestClient _client;
        private IRestResponse _response;
        private Guid _identifier;
        private MockWebEndpoint _restEndpoint;

        public UI_pages(ITestOutputHelper output)
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

        private void Given_a_monitor_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_a_page_via_url(string url)
        {
            _response = _client.ExpectAnSuccessfulGet(url);
        }

        private void Then_the_home_page_should_be_returned()
        {
            Assert.Contains("<title>Health Monitoring</title>",_response.Content);
        }

        private void Then_the_dashboard_page_should_be_returned()
        {
            Assert.Contains("<title>Health Monitoring - dashboard</title>", _response.Content);
        }

        private void Then_the_endpoint_details_page_should_be_returned()
        {
            Assert.Contains("<title>Health Monitoring - {{details.Group}}:{{details.Name}}</title>", _response.Content);
        }

        private void Given_a_registered_endpoint()
        {
            _restEndpoint = MockWebEndpointFactory.CreateNew();
            _identifier = _client.RegisterEndpoint(MonitorTypes.Http, _restEndpoint.StatusAddress, "test","test");
        }
    }
}