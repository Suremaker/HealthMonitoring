using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor system effectively
As ops
I want to see the endpoints pushing their health status to the Health Monitor")]
    public partial class Push_endpoint_monitoring
    {
        [Scenario]
        public void Registering_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_registration_metadata_should_be_provided()
                );
        }

        [Scenario]
        public void Receiving_healthy_status_updates()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => Then_monitor_should_receive_updates_within_configured_time_intervals()
                );
        }

        [Scenario]
        public void Receiving_different_status_updates()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_endpoint_health_check_method_detect_health_issues_and_return_unhealthy_status(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Unhealthy),
                _ => When_endpoint_health_check_method_detect_health_critical_issues_and_return_faulty_status(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Faulty),
                _ => When_endpoint_health_check_method_detect_endpoint_being_put_offline_and_return_offline_status(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Offline)
                );
        }

        [Scenario]
        public void Receiving_endpoint_additional_details()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => Given_endpoint_is_configured_to_provide_additional_details_to_Health_Monitor(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_additional_details_should_be_provided()
                );
        }

        [Scenario]
        public void Detecting_endpoint_health_update_timeout()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_endpoint_terminates_so_that_it_no_longer_sends_updates(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.TimedOut)
                );
        }

        [Scenario]
        public void Re_registering_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_with_push_integration(),
                _ => When_endpoint_starts(),
                _ => Then_endpoint_should_be_registered_in_Health_Monitor(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_admin_removes_the_endpoint_from_the_monitor(),
                _ => Then_endpoint_should_register_itself_again()
                );
        }
    }
}