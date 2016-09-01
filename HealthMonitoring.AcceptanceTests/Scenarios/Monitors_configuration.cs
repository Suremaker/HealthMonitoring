using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to register services for monitoring
As ops
I want to know what kind of monitor types are supported")]
    public partial class Monitors_configuration
    {
        [Scenario]
        public void Retrieving_list_of_supported_monitors()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_a_list_of_supported_monitors_via_url("/api/monitors"),
                _ => Then_a_list_of_supported_monitors_should_be_returned__LIST("http", "http.json", "nsb3", "nsb5.msmq", "nsb5.rabbitmq"));
        }
    }
}