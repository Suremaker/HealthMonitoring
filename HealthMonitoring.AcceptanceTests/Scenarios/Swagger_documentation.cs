using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

namespace HealthMonitoring.AcceptanceTests.Scenarios
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
                _ => Given_a_monitor_client(),
                _ => When_client_requests_a_swagger_documentation_via_url("/swagger/ui/index"),
                _ => Then_the_swagger_API_documentation_should_be_returned());
        }

        [Scenario]
        public void Redirecting_to_the_documentation()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_client(),
                _ => When_client_requests_an_api_description_via_url("/api"),
                _ => Then_the_client_should_be_redirected_to_url("/swagger/ui/index"));
        }
    }
}