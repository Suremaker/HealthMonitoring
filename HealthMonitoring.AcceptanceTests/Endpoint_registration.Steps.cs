using System;
using System.Linq;
using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;
using Newtonsoft.Json;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests
{
    public partial class Endpoint_registration : FeatureFixture
    {
        private RestClient _client;
        private IRestResponse _response;
        private Guid _identifier;

        public Endpoint_registration(ITestOutputHelper output)
            : base(output)
        {
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_endpoint_registration_via_url_with_name_address_group_and_protocol(string url, string name, string address, string @group, string protocol)
        {
            _response = _client.Post(new RestRequest(url).AddJsonBody(new { group, protocol, name, address }));
        }

        private void Then_a_new_endpoint_identifier_should_be_returned()
        {
            _response.VerifyValidStatus(HttpStatusCode.Created);
            _identifier = JsonConvert.DeserializeObject<Guid>(_response.Content);
            Assert.NotEqual(Guid.Empty, _identifier);
            _response.VerifyLocationHeader(string.Format("api/endpoints/{0}", _identifier));
        }
    }
}