using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

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
        public void Updating_existing_endpoint_without_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2525/status", "group", MonitorTypes.HttpJson, _credentials.PersonalCredentials.Password),
                _ => When_client_request_endpoint_update_without_personal_credentials("registered", "http://localhost:2525/status", "group1", MonitorTypes.HttpJson),
                _ => Then_client_should_receive_STATUS_code(HttpStatusCode.Unauthorized));
        }

        [Scenario]
        public void Updating_existing_endpoint_with_personal_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2526/status", "group", MonitorTypes.HttpJson, _credentials.PersonalCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_client_request_endpoint_update_with_credentials("registered", "http://localhost:2526/status", "group1", MonitorTypes.HttpJson, null, _credentials.PersonalCredentials.Password),
                _ => Then_a_new_endpoint_identifier_should_be_returned());
        }

        [Scenario]
        public void Trying_to_update_existing_endpoint_by_another_one()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2527/status", "group", MonitorTypes.HttpJson, _credentials.GenerateRandomCredentials().Password),
                _ => Given_endpoint_id_is_received(),
                _ => Given_endpoint_with_password_is_registered("registered", "http://localhost:2528/status", "group", MonitorTypes.HttpJson, _credentials.PersonalCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_client_request_endpoint_update_with_credentials("registered", "http://localhost:2527/status", "group1", MonitorTypes.HttpJson, null, _credentials.PersonalCredentials.Password),
                _ => Then_client_should_receive_STATUS_code(HttpStatusCode.Unauthorized));
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
        public void Registering_new_endpoint_with_short_private_key()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_request_endpoint_registration_with_short_password(),
                _ => Then_client_should_receive_STATUS_code(HttpStatusCode.BadRequest)
                );
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
        public void Filtering_endpoints()
        {
            var groupBase = nameof(Filtering_endpoints);
            var group1 = groupBase + "_g1";
            var group2 = groupBase + "_g2";

            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_healthy_endpoint_with_name_group_and_tags("my_name1", group1, new[] { "tag1" }),
                _ => Given_a_faulty_endpoint_with_name_group_and_tags("my_name2", group1, new[] { "tag1", "tag2" }),
                _ => Given_a_offline_endpoint_with_name_group_and_tags("other_name", group2, new[] { "tag1", "tag2", "tag3" }),

                _ => When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER("/api/endpoints", group1),
                _ => Then_returned_endpoint_list_should_contain_endpoints("my_name1", "my_name2"),

                _ => When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER("/api/endpoints", groupBase + "*"),
                _ => Then_returned_endpoint_list_should_contain_endpoints("my_name1", "my_name2", "other_name"),

                _ => When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_tag_filter_TAGFILTER("/api/endpoints", groupBase + "*", new[] { "tag1", "tag2" }),
                _ => Then_returned_endpoint_list_should_contain_endpoints("my_name2", "other_name"),

                _ => When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_status_filter_STATUSFILTER("/api/endpoints", groupBase + "*", new[] { EndpointStatus.Offline, EndpointStatus.Faulty }),
                _ => Then_returned_endpoint_list_should_contain_endpoints("my_name2", "other_name"),

                _ => When_client_requests_all_endpoints_details_via_url_and_group_filter_GROUPFILTER_as_well_as_text_filter_TEXTFILTER("/api/endpoints", groupBase + "*", "lt*y"),
                _ => Then_returned_endpoint_list_should_contain_endpoints("my_name1", "my_name2")
                );
        }

        [Scenario]
        public void Deleting_existing_endpoint_with_admin_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("already_registered", "http://localhost:3033/status", "my_group", MonitorTypes.HttpJson),
                _ => When_client_requests_endpoint_deletion_via_url_with_admin_credentials("/api/endpoints/" + _identifier),
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

        [Scenario]
        public void Updating_endpoint_tags_with_admin_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("my_name", "http://localhost:3034/status", "my_group", MonitorTypes.HttpJson),
                _ => When_client_requests_tags_updating_via_url_with_admin_credentials($"api/endpoints/{_identifier}/tags", new[] { "tag1", "tag2" }),
                _ => Then_status_should_be_returned(HttpStatusCode.OK),
                _ => When_client_requests_endpoint_details_via_url("/api/endpoints/" + _identifier),
                _ => Then_the_endpoint_tags_should_be(new[] { "tag1", "tag2" })
                );
        }

        [Scenario]
        public void Updating_endpoint_tags_with_personal_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_password_is_registered("my_name", "http://localhost:3035/status", "my_group", MonitorTypes.HttpJson,
                _credentials.PersonalCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_client_requests_tags_updating_via_url_with_personal_credentials($"api/endpoints/{_identifier}/tags", new[] { "tag1", "tag2" }),
                _ => Then_status_should_be_returned(HttpStatusCode.OK),
                _ => When_client_requests_endpoint_details_via_url("/api/endpoints/" + _identifier),
                _ => Then_the_endpoint_tags_should_be(new[] { "tag1", "tag2" })
                );
        }

        [Scenario]
        public void Requesting_endpoint_identity()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_password_is_registered("my_name1", "http://localhost:3036/status", "my_group", MonitorTypes.HttpJson,
                _credentials.PersonalCredentials.Password),
                _ => Given_endpoint_id_is_received(),
                _ => When_client_requests_all_endpoints_details_via_url("/api/endpoints/identities"),
                _ => Then_response_should_contain_only_id_and_address_and_monitortype(_credentials.PersonalCredentials.Id, "http://localhost:3036/status", MonitorTypes.HttpJson)
                );
        }

        [Scenario]
        public void PostEndpointHealth_with_admin_credentials()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_endpoint_with_name_address_group_and_monitor_is_registered("endpoint", "someaddress", "group", MonitorTypes.HttpJson),
                _ => Given_endpoint_id_is_received(),
                _ => When_client_request_endpoint_health_update_with_credentials(_credentials.PersonalCredentials.Id, 
                EndpointStatus.Offline, _credentials.AdminCredentials),
                _ => Then_client_should_receive_STATUS_code(HttpStatusCode.Forbidden)
                );
        }
    }
}