using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor endpoints
As ops
I want to be able to register browse and unregister endpoints")]
    public partial class Endpoint_registration
    {
        [Scenario]
        public void Registering_new_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_endpoint_registration_via_url_with_name_address_group_and_monitor(ClientHelper.EndpointRegistrationUrl, "my_name", "http://localhost:2524/status", "my_group", MonitorTypes.HttpJson),
                _ => Then_a_new_endpoint_identifier_should_be_returned(),
                _ => When_client_requests_endpoint_details_via_url("/api/endpoints/" + _identifier),
                _ => Then_endpoint_information_should_be_returned_including_name_address_group_and_monitor("my_name", "http://localhost:2524/status", "my_group", MonitorTypes.HttpJson));
        }

        [Scenario]
        public void Registering_new_endpoint_with_unsupported_monitor()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_endpoint_registration_via_url_with_name_address_group_and_monitor(ClientHelper.EndpointRegistrationUrl, "my_name", "http://localhost:2524/status", "my_group", MonitorTypes.Unsupported),
                _ => Then_client_should_receive_STATUS_code(HttpStatusCode.BadRequest),
                _ => Then_response_should_contain_message("Unsupported monitor: " + MonitorTypes.Unsupported));
        }

        [Scenario]
        public void Retrieving_inexistent_endpoint_definition()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_endpoint_details_for_inexistent_endpoint_identifier(),
                _ => Then_status_should_be_returned(HttpStatusCode.NotFound));
        }

        [Scenario]
        public void Retrieving_all_endpoints()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("my_name1", "http://localhost:3031/status", "my_group", MonitorTypes.HttpJson),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("my_name2", "http://localhost:3032/status", "my_group", MonitorTypes.HttpJson),
                _ => When_client_requests_all_endpoints_details_via_url("/api/endpoints"),
                _ => Then_returned_endpoint_list_should_include_endpoint_with_name_address_group_and_monitor("my_name1", "http://localhost:3031/status", "my_group", MonitorTypes.HttpJson),
                _ => Then_returned_endpoint_list_should_include_endpoint_with_name_address_group_and_monitor("my_name2", "http://localhost:3032/status", "my_group", MonitorTypes.HttpJson));
        }

        [Scenario]
        public void Deleting_existing_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("my_name", "http://localhost:3333/status", "my_group", MonitorTypes.HttpJson),
                _ => When_client_requests_endpoint_deletion_via_url("/api/endpoints/" + _identifier),
                _ => Then_status_should_be_returned(HttpStatusCode.OK),
                _ => When_client_requests_endpoint_details_for_inexistent_endpoint_identifier(),
                _ => Then_status_should_be_returned(HttpStatusCode.NotFound)
                );
        }

        [Scenario]
        public void Deleting_inexistent_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_endpoint_deletion_for_inexistent_endpoint_identifier(),
                _ => Then_status_should_be_returned(HttpStatusCode.NotFound)
                );
        }
    }
}