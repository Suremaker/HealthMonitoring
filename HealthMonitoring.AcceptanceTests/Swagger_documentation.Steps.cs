using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests
{
    public partial class Swagger_documentation : FeatureFixture
    {
        private RestClient _client;
        private IRestResponse _response;

        public Swagger_documentation(ITestOutputHelper output)
            : base(output)
        {
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_a_swagger_documentation_via_URL(string url)
        {
            _response = _client.ExpectAnSuccessfulGet(url);
        }

        private void Then_the_swagger_API_documentation_is_returned()
        {
            Assert.Contains("<title>Swagger UI</title>", _response.Content);
        }
    }
}