using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD.Framework.Formatting;
using LightBDD.XUnit2;
using Newtonsoft.Json;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Monitors_configuration : FeatureFixture
    {
        private RestClient _client;
        private IRestResponse _response;

        public Monitors_configuration(ITestOutputHelper output)
            : base(output)
        {
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_a_list_of_supported_monitors_via_url(string url)
        {
            _response = _client.ExpectAnSuccessfulGet(url);
        }

        private void Then_a_list_of_supported_monitors_should_be_returned__LIST([FormatCollection] params string[] list)
        {
            var types = JsonConvert.DeserializeObject<string[]>(_response.Content);
            Assert.Equal(list, types);
        }
    }
}