using LightBDD;

namespace HealthMonitoring.AcceptanceTests
{
    [FeatureDescription(
@"In order to register services for monitoring
As ops
I want to know what kind of protocols are supported")]
    public partial class Protocols_configuration
    {
        [Scenario]
        public void Retrieving_list_of_supported_protocols()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_a_list_of_supported_protocols_via_url("/api/protocols"),
                _ => Then_a_list_of_supported_protocols_should_be_returned__LIST("rest"));
        }
    }
}