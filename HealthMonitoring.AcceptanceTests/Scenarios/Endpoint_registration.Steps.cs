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

namespace HealthMonitoring.AcceptanceTests.Scenarios
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

        private void When_client_requests_endpoint_registration_via_url_with_name_address_group_and_monitor(string url, string name, string address, string @group, string monitor)
        {
            _response = _client.Post(new RestRequest(url).AddJsonBody(new { group, monitorType = monitor, name, address }));
        }

        private void Then_a_new_endpoint_identifier_should_be_returned()
        {
            _response.VerifyValidStatus(HttpStatusCode.Created);
            _identifier = JsonConvert.DeserializeObject<Guid>(_response.Content);
            Assert.NotEqual(Guid.Empty, _identifier);
            _response.VerifyLocationHeader($"api/endpoints/{_identifier}");
        }

        private void When_client_requests_endpoint_details_via_url(string url)
        {
            _response = _client.Get(new RestRequest(url));
        }

        private void Then_endpoint_information_should_be_returned_including_name_address_group_and_monitor(string name, string address, string group, string monitor)
        {
            var entity = _response.DeserializeEndpointDetails();
            AssertEntity(entity, name, address, group, monitor);
        }

        private static void AssertEntity(EndpointEntity entity, string name, string address, string group, string monitor)
        {
            Assert.Equal(name, entity.Name);
            Assert.Equal(address, entity.Address);
            Assert.Equal(group, entity.Group);
            Assert.Equal(monitor, entity.MonitorType);
        }

        private void When_client_requests_endpoint_details_for_inexistent_endpoint_identifier()
        {
            When_client_requests_endpoint_details_via_url($"api/endpoints/{Guid.NewGuid()}");
        }

        private void Then_status_should_be_returned(HttpStatusCode status)
        {
            _response.VerifyValidStatus(status);
        }

        private void Given_endpoint_with_name_address_group_and_monitor_is_registered(string name, string address, string group, string monitor)
        {
            When_client_requests_endpoint_registration_via_url_with_name_address_group_and_monitor("/api/endpoints/register", name, address, group, monitor);
            Then_a_new_endpoint_identifier_should_be_returned();
        }

        private void When_client_requests_all_endpoints_details_via_url(string url)
        {
            _response = _client.Get(new RestRequest(url));
        }

        private void Then_returned_endpoint_list_should_include_endpoint_with_name_address_group_and_monitor(string name, string address, string group, string monitor)
        {
            _response.VerifyValidStatus(HttpStatusCode.OK);
            var entities = JsonConvert.DeserializeObject<EndpointEntity[]>(_response.Content);
            var entity = entities.SingleOrDefault(e => e.MonitorType == monitor && e.Address == address);
            AssertEntity(entity, name, address, group, monitor);
        }

        private void Then_client_should_receive_STATUS_code(HttpStatusCode status)
        {
            _response.VerifyValidStatus(status);
        }

        private void Then_response_should_contain_message(string message)
        {
            var error = JsonConvert.DeserializeObject<ErrorEntity>(_response.Content).Message;
            Assert.Equal(message, error);
        }

        private void When_client_requests_endpoint_deletion_via_url(string url)
        {
            _response = _client.Delete(new RestRequest(url));
        }

        private void When_client_requests_endpoint_deletion_for_inexistent_endpoint_identifier()
        {
            When_client_requests_endpoint_deletion_via_url($"api/endpoints/{Guid.NewGuid()}");
        }
        private void When_client_requests_tags_updating_via_url(string url, string[] tags)
        {
            _response = _client.Post(new RestRequest(url).AddJsonBody(tags));
        }

        private void Then_endpoint_tags_should_be_updated(string[] tags)
        {
            EndpointEntity ed  = _response.DeserializeEndpointDetails();

            AssertTags(ed.Tags, tags);
        }

        private void AssertTags(string[] existing, string[] expected)
        {
            Assert.Equal(existing.Length, expected.Length);
            existing = existing.OrderBy(x => x).ToArray();
            expected = expected.OrderBy(x => x).ToArray();

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], existing[i]);
            }
        }
    }
}