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
        public void Details_page_structure_test()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_endpoint_name(),
                _ => And_elements_are_rendered(),
                _ => Then_name_and_group_should_be_the_same_as_on_home_page()
                );

        }
    }
}