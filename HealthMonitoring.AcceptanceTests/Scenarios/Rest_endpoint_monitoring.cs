using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor system effectively
As ops
I want to monitor registered rest (http.json) endpoints")]
    public partial class Rest_endpoint_monitoring
    {
        [Scenario]
        public void Monitoring_rest_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_rest_endpoint(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided()
                );
        }

        [Scenario]
        public void Monitoring_inexistent_rest_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_that_has_not_been_deployed_yet(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.NotExists),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.NotExists),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_be_not_available()
                );
        }

        [Scenario]
        public void Monitoring_offline_rest_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_is_offline(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Offline),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Offline),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_be_not_available()
                );
        }

        [Scenario]
        public void Monitoring_healthy_rest_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_healthy_rest_endpoint(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Healthy),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_be_provided()
                );
        }

        [Scenario]
        public void Monitoring_faulty_rest_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_healthy_rest_endpoint(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy),
                _ => When_endpoint_becomes_faulty(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Faulty),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Faulty),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_contain_error_information()
                );
        }
    }
}