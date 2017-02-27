using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    [FeatureDescription(
@"In order to monitor endpoints effectively
As user
I want to be able to see navigate through Health Monitor UI pages")]
    public partial class UI_pages
    {
        [Scenario]
        public void Retrieving_home_page()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_client(),
                _ => When_client_requests_a_page_via_url("/"),
                _ => Then_the_home_page_should_be_returned());
        }

        [Scenario]
        public void Retrieving_dashboard_page()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_client(),
                _ => When_client_requests_a_page_via_url("/dashboard"),
                _ => Then_the_dashboard_page_should_be_returned());
        }

        [Scenario]
        public void Retrieving_endpoint_details_page()
        {
            Runner.RunScenario(
                _ => Given_a_monitor_client(),
                _ => Given_a_registered_endpoint(),
                _ => When_client_requests_a_page_via_url("/dashboard/details?id=" + _identifier),
                _ => Then_the_endpoint_details_page_should_be_returned());
        }
    }
}