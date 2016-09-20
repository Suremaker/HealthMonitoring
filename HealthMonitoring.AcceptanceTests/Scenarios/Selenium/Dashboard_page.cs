using LightBDD;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(
        @"In order to view all minitoring services
        As User 
        I want to open dashboard page")]
    public partial class Dashboard_page
    {
        [Scenario]
        public void Homepage_link_test()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => When_user_clicked_on_home_link(),
                _ => Then_home_page_is_opened()
                );
        }

        [Scenario]
        public void Status_filter_test()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => Given_rendered_endpoint_tiles(),
                _ => When_user_clicked_on_status_select(),
                _ => And_user_selected_healthy_and_faulty_statuses(),
                _ => Then_only_healthy_and_faulty_endpoints_should_be_displayed(),
                _ => And_filter_should_be_displayed_in_page_url()
                );
        }

        [Scenario]
        public void Status_filter_from_url_test()
        {
            Runner.RunScenario(
                _ => Given_healthy_and_faulty_statuses_in_url_filter(),
                _ => And_load_page(),
                _ => Then_only_healthy_and_faulty_endpoints_should_be_displayed()
                );
        }

        [Scenario]
        public void Group_view_test()
        {
            Runner.RunScenario(
                _ => Given_dashboard_page(),
                _ => Given_rendered_endpoint_tiles(),
                _ => When_user_clicked_on_group_view_checkbox(),
                _ => Then_endpoints_are_grouped(),
                _ => And_all_endpoints_in_subgroup_have_the_same_group()
                );
        }

        [Scenario]
        public void Url_wildcard_group_filtering_test()
        {
            Runner.RunScenario(
                _ => Given_wildcard_group_filter_in_url(),
                _ => When_user_navigates_to_dashboard_page_with_group_filter_in_url(),
                _ => Then_are_shown_endpoints_with_group_that_satisfy_filter()
                );
        }

        [Scenario]
        public void Wildcard_group_filtering_test()
        {
            Runner.RunScenario(
                 _ => Given_dashboard_page(),
                 _ => When_user_writes_group_into_input(),
                 _ => Then_are_shown_endpoints_with_group_that_satisfy_filter(),
                 _ => And_group_filter_is_appended_to_url()
                );
        }
    }
}