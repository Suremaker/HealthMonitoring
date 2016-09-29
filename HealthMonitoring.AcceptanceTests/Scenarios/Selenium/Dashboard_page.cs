using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(
@"In order to view all monitoring services
As User 
I want to open dashboard page")]
    public partial class Dashboard_page
    {
        [Scenario]
        public void Verification_of_homepage_link()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_home_link(),
                _ => Then_home_page_should_open()
                );
        }

        [Scenario]
        public void Applying_status_filter_to_endpoints()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_status_multiselect_element(),
                _ => When_user_selects_healthy_and_faulty_statuses(),
                _ => Then_only_healthy_and_faulty_endpoints_should_be_displayed(),
                _ => Then_filter_should_be_displayed_in_page_url()
                );
        }

        [Scenario]
        public void Applying_status_filter_via_url()
        {
            Runner.RunScenario(
                _ => Given_healthy_and_faulty_statuses_in_url_filter(),
                _ => When_user_loads_page_with_filter_in_url(),
                _ => Then_only_healthy_and_faulty_endpoints_should_be_displayed()
                );
        }

        [Scenario]
        public void Enabling_group_view_mode()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_group_view_checkbox(),
                _ => Then_endpoints_are_grouped(),
                _ => Then_all_endpoints_in_subgroup_should_have_the_same_group()
                );
        }

        [Scenario]
        public void Navigating_to_dashboard_page_with_wildcard_group_filter_in_url()
        {
            Runner.RunScenario(
                _ => Given_wildcard_group_filter_in_url(),
                _ => When_user_navigates_to_dashboard_page_with_group_filter_in_url(),
                _ => Then_should_be_displayed_endpoints_with_groupes_that_satisfy_filter()
                );
        }

        [Scenario]
        public void Applying_wildcard_filter_in_group_input()
        {
            Runner.RunScenario(
                 _ => Given_dashboard_page(),
                 _ => Given_endpoints_are_visible(),
                 _ => When_user_writes_group_into_input(),
                 _ => Then_should_be_displayed_endpoints_with_groupes_that_satisfy_filter(),
                 _ => Then_group_filter_should_be_appended_to_url()
                );
        }
    }
}