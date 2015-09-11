using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;
using Newtonsoft.Json;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests
{
    public partial class Protocols_configuration : FeatureFixture
    {
        private RestClient _client;
        private IRestResponse _response;

        public Protocols_configuration(ITestOutputHelper output)
            : base(output)
        {
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_a_list_of_supported_protocols_via_url(string url)
        {
            _response = _client.ExpectAnSuccessfulGet(url);
        }

        private void Then_a_list_of_supported_protocols_should_be_returned__LIST([FormatCollection] params string[] list)
        {
            var types = JsonConvert.DeserializeObject<string[]>(_response.Content);
            Assert.Equal(list, types);
        }
    }
}