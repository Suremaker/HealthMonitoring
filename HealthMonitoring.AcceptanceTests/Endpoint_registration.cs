using HealthMonitoring.AcceptanceTests.Helpers;
using LightBDD;

namespace HealthMonitoring.AcceptanceTests
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
                _ => When_client_requests_endpoint_registration_via_url_with_name_address_group_and_protocol("/api/endpoints/register", "my_name", "http://localhost:2524/status", "my_group", Protocols.Rest),
                _ => Then_a_new_endpoint_identifier_should_be_returned());
        }
    }
}