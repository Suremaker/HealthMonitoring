using System;
using System.Linq;
using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Entities;
using HealthMonitoring.AcceptanceTests.Helpers.Http;
using LightBDD;
using Newtonsoft.Json;
using RestSharp;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Endpoint_registration : FeatureFixture, IDisposable
    {
        private RestClient _client;
        private IRestResponse _response;
        private Guid _identifier;
        private readonly List<MockWebEndpoint> _endpoints = new List<MockWebEndpoint>();

        public Endpoint_registration(ITestOutputHelper output)
            : base(output)
        {
        }

        public void Dispose()
        {
            foreach (var endpoint in _endpoints)
                endpoint.Dispose();
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_requests_endpoint_registration_via_url_with_name_address_group_and_monitor(string url, string name, string address, string group, string monitor)
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
            _response = _client.Put(new RestRequest(url).AddJsonBody(tags));
        }

        private void Then_the_endpoint_tags_should_be(string[] tags)
        {
            var entity = _response.DeserializeEndpointDetails();
            AssertTags(tags, entity.Tags);
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

        private void Given_a_healthy_endpoint_with_name_group_and_tags(string name, string group, [FormatCollection] string[] tags)
        {
            SetupHttpEndpoint(HttpStatusCode.OK, name, group, tags, EndpointStatus.Healthy);
        }

        private void Given_a_faulty_endpoint_with_name_group_and_tags(string name, string group, [FormatCollection] string[] tags)
        {
            SetupHttpEndpoint(HttpStatusCode.InternalServerError, name, group, tags, EndpointStatus.Faulty);
        }

        private void Given_a_offline_endpoint_with_name_group_and_tags(string name, string group, [FormatCollection] string[] tags)
        {
            SetupHttpEndpoint(HttpStatusCode.ServiceUnavailable, name, group, tags, EndpointStatus.Offline);
        }

        private void SetupHttpEndpoint(HttpStatusCode httpStatusCode, string name, string group, string[] tags, EndpointStatus expectedStatus)
        {
            var endpoint = MockWebEndpointFactory.CreateNew();
            _endpoints.Add(endpoint);

            endpoint.SetupStatusResponse(httpStatusCode);
            var identifier = _client.RegisterEndpoint(MonitorTypes.Http, endpoint.StatusAddress, group, name, tags);
            _client.EnsureStatusChanged(identifier, expectedStatus);
        }

        private void When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER(string url, string groupFilter)
        {
            _response = _client.Get(new RestRequest(url).AddQueryParameter("filterGroup", groupFilter));
        }

        private void When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_tag_filter_TAGFILTER(string url, string groupFilter, [FormatCollection] string[] tagFilter)
        {
            var request = new RestRequest(url).AddQueryParameter("filterGroup", groupFilter);
            foreach (var tag in tagFilter)
                request.AddQueryParameter("filterTags", tag);

            _response = _client.Get(request);
        }

        private void When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_status_filter_STATUSFILTER(string url, string groupFilter, [FormatCollection]EndpointStatus[] statusFilter)
        {
            var request = new RestRequest(url).AddQueryParameter("filterGroup", groupFilter);
            foreach (var status in statusFilter)
                request.AddQueryParameter("filterStatus", status.ToString());

            _response = _client.Get(request);
        }

        private void When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_text_filter_TEXTFILTER(string url, string groupFilter, string textFilter)
        {
            _response = _client.Get(new RestRequest(url).AddQueryParameter("filterGroup", groupFilter).AddQueryParameter("filterText", textFilter));
        }

        private void Then_returned_endpoint_list_should_contain_endpoints([FormatCollection]params string[] endpoints)
        {
            _response.VerifyValidStatus(HttpStatusCode.OK);
            var actual = JsonConvert.DeserializeObject<EndpointEntity[]>(_response.Content).Select(e => e.Name).OrderBy(n => n).ToArray();
            var expected = endpoints.OrderBy(e => e).ToArray();
            Assert.Equal(expected, actual);
        }
    }
}