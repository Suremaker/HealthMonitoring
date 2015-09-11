using System;
using System.Linq;
using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Entities;
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

        private void When_client_requests_endpoint_details_via_url(string url)
        {
            _response = _client.Get(new RestRequest(url));
        }

        private void Then_endpoint_information_should_be_returned_including_name_address_group_and_protocol(string name, string address, string group, string protocol)
        {
            _response.VerifyValidStatus(HttpStatusCode.OK);
            var entity = JsonConvert.DeserializeObject<EndpointEntity>(_response.Content);
            AssertEntity(entity, name, address, group, protocol);
        }

        private static void AssertEntity(EndpointEntity entity, string name, string address, string group, string protocol)
        {
            Assert.Equal(name, entity.Name);
            Assert.Equal(address, entity.Address);
            Assert.Equal(group, entity.Group);
            Assert.Equal(protocol, entity.Protocol);
        }

        private void When_client_requests_endpoint_details_for_inexistent_endpoint_identifier()
        {
            When_client_requests_endpoint_details_via_url(string.Format("api/endpoints/{0}", Guid.NewGuid()));
        }

        private void Then_status_should_be_returned(HttpStatusCode status)
        {
            _response.VerifyValidStatus(status);
        }

        private void Given_endpoint_with_name_address_group_and_protocol_is_registered(string name, string address, string group, string protocol)
        {
            When_client_requests_endpoint_registration_via_url_with_name_address_group_and_protocol("/api/endpoints/register", name, address, group, protocol);
        }

        private void When_client_requests_all_endpoints_details_via_url(string url)
        {
            _response = _client.Get(new RestRequest(url));
        }

        private void Then_returned_endpoint_list_should_include_endpoint_with_name_address_group_and_protocol(string name, string address, string group, string protocol)
        {
            _response.VerifyValidStatus(HttpStatusCode.OK);
            var entities = JsonConvert.DeserializeObject<EndpointEntity[]>(_response.Content);
            var entity = entities.SingleOrDefault(e => e.Protocol == protocol && e.Address == address);
            AssertEntity(entity, name, address, group, protocol);
        }
    }
}