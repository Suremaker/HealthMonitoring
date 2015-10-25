using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor endpoints effectively
As ops
I want to be able to see endpoint statistics")]
    public partial class Endpoint_statistics
    {
        [Scenario]
        public void Retrieving_endpoint_statistics()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_a_healthy_endpoint_working_for_some_time(),
                _ => When_client_requests_endpoint_statistics_via_url("/api/endpoints/" + _identifier + "/stats"),
                _ => Then_the_response_should_containing_endpoint_statistics(),
                _ => Then_statistics_should_be_ordered_ascending_by_check_time()
                );
        }
    }
}