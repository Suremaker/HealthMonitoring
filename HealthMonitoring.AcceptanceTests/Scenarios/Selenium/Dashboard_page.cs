using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(@"Selenium tests for dashboard page")]
    public partial class Dashboard_page
    {
        [Scenario]
        public void Homepage_link_test()
        {
            Runner.RunScenario(
                _ => Load_dashboard_page(),
                _ => Click_on_menu_header_should_redirect_to_home_page()
                );
        }

        [Scenario]
        public void Status_filter_test()
        {
            Runner.RunScenario(
                _ => Load_dashboard_page(),
                _ => Status_filter_should_select_only_healthy_and_faulty_endpoints()
                );
        }

        [Scenario]
        public void Status_filter_from_url_test()
        {
            Runner.RunScenario(
                _ => Load_dashboard_page(),
                _ => Status_filter_should_be_applied_from_url_parameter()
                );
        }

        [Scenario]
        public void Group_view_test()
        {
            Runner.RunScenario(
                _ => Load_dashboard_page(),
                _ => Group_view_should_join_endpoints_into_groups()
                );
        }

        [Scenario]
        public void Url_wildcard_group_filtering_test()
        {
            Runner.RunScenario(
                _ => Group_filter_should_apply_wildcards_from_url()
                );
        }

        [Scenario]
        public void Wildcard_group_filtering_test()
        {
            Runner.RunScenario(
                 _ => Load_dashboard_page(),
                 _ => Group_filter_should_apply_wildcards()
                );
        }
    }
}