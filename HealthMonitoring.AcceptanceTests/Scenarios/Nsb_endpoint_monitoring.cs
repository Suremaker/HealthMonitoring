using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor system effectively
As ops
I want to monitor registered nsb endpoints")]
    public partial class Nsb_endpoint_monitoring
    {
        [Scenario]
        public void Monitoring_inexistent_or_faulty_nsb_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_that_has_not_been_deployed_yet(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),

                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.TimedOut),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.TimedOut),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_contain_timeout_information(),

                _ => When_more_time_pass(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Faulty),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Faulty),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_contain_timeout_information()
                );
        }

        [Scenario]
        public void Monitoring_nsb_endpoint_with_connection_issues()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_unreachable_endpoint(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Faulty),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Faulty),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_contain_failure_information()
                );
        }

        [Scenario]
        public void Monitoring_healthy_nsb_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_healthy_nsb_endpoint(),
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
    }
}