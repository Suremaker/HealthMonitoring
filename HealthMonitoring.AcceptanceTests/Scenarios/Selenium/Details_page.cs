using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(
@"In order to view endpoint details
As User 
I want to open details page")]
    public partial class Details_page
    {
        [Scenario]
        public void Navigating_from_home_to_details_page()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicks_on_endpoint_name(),
                _ => When_elements_on_details_page_are_rendered(),
                _ => Then_name_and_group_and_tags_should_be_the_same_as_on_home_page()
                );

        }
    }
}