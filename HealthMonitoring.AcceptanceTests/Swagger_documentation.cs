using LightBDD;

namespace HealthMonitoring.AcceptanceTests
{
    [FeatureDescription(
@"In order to understand how to use the service
As a user
I want to retrieve swagger API documentation")]
    public partial class Swagger_documentation
    {
        [Scenario]
        public void Retrieving_swagger_documentation()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_api_client(),
                _ => When_client_requests_a_swagger_documentation_via_URL("/swagger/ui/index"),
                _ => Then_the_swagger_API_documentation_is_returned());
        }
    }
}