using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor system deployed to multiple networks
As ops
I want to deploy multiple monitor processes
And register endpoints to use monitors that are reachable in given network")]
    public partial class Endpoint_monitoring_with_specific_monitor_process
    {
        [Scenario]
        public void Monitoring_endpoint_with_specific_monitor_process()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => Given_an_healthy_endpoint_that_has_not_been_deployed_yet(),
                _ => When_client_registers_the_endpoint_with_specific_monitor_tag(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS_with_error(EndpointStatus.TimedOut, "Endpoint health was not updated within specified period of time."),
                _ => When_monitor_process_with_that_tag_starts(),
                _ => Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus.Healthy)
                );
        }
    }
}