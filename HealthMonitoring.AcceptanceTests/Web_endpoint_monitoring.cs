using LightBDD;
using HealthMonitoring.AcceptanceTests.Helpers;

namespace HealthMonitoring.AcceptanceTests
{
    [FeatureDescription(
@"In order to monitor system effectively
As ops
I want to monitor registered web endpoints")]
    public partial class Web_endpoint_monitoring
    {
        [Scenario]
        public void Monitoring_inexistent_web_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_endpoint_that_has_not_been_deployed_yet(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Inactive),
                _ => When_client_requests_endpoint_details(),
                _ => Then_the_endpoint_status_should_be_provided(EndpointStatus.Inactive),
                _ => Then_the_last_check_time_should_be_provided(),
                _ => Then_the_response_time_should_be_provided(),
                _ => Then_the_endpoint_additional_details_should_be_not_available()
                );
        }

        [Scenario]
        public void Monitoring_healthy_web_endpoint()
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
                _ => Then_the_endpoint_additional_details_should_be_not_available()
                );
        }

        [Scenario]
        public void Monitoring_faulty_web_endpoint()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_unhealthy_rest_endpoint(),
                _ => When_client_registers_the_endpoint(),
                _ => Then_monitor_should_start_monitoring_the_endpoint(),
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