using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(@"Selenium tests for details page")]
    public partial class Details_page
    {
        [Scenario]
        public void Details_page_structure_test()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => Navigate_to_details_page(),
                _ => Details_page_should_display_correct_endpoint_name_end_status()
                );
        }
    }
}