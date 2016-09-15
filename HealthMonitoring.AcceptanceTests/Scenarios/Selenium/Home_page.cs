using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(@"Selenium tests for home page")]
    public partial class Home_page
    {

        [Scenario]
        public void Verification_of_page_title()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => Verify_page_title()
                );
        }

        [Scenario]
        public void Verification_of_dashboard_menu_links()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => Verify_dashboard_link(),
                _ => Load_home_page(),
                _ => Verify_swagger_link(),
                _ => Load_home_page(),
                _ => Verify_github_link()
                );
        }

        [Scenario]
        public void Status_filter_test()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => After_click_on_status_should_be_displayed_endpoints_with_selected_status()
                );
        }

        [Scenario]
        public void Tag_filter_test()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => After_clicking_on_tag_should_be_displayed_endpoints_with_selected_tag()
                );
        }

        [Scenario]
        public void Url_filter_test()
        {
            Runner.RunScenario(
                _ => Load_home_page(),
                _ => Url_filters_should_be_applied_to_endpoints()
                );
        }
    }
}