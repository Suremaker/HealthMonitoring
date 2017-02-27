using System;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    [FeatureDescription(
@"In order to understend how to use HealthMoniting UI
As User 
I want to open home page")]
    public partial class Home_page
    {
        [Scenario]
        public void Verification_of_page_title()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => Then_page_should_contain_title()
                );
        }

        [Scenario]
        public void Verification_of_dashboard_link()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_dashboad_page_link(),
                _ => Then_dashboard_page_should_be_opened()
                );
        }

        [Scenario]
        public void Verification_of_swagger_link()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_swagger_page_link(),
                _ => Then_swagger_page_should_be_opened()
                );
        }

        [Scenario]
        public void Verification_of_project_link()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_project_page_link(),
                _ => Then_project_page_should_be_opened()
                );
        }

        [Scenario]
        public void Applying_status_filter_to_endpoints()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_status_button(),
                _ => Then_status_filter_should_be_appended_to_url(),
                _ => Then_only_endpoints_with_chosen_status_should_be_shown(),
                _ => Then_should_be_shown_selected_status()
                );
        }

        [Scenario]
        public void Applying_tag_filter_to_endpoints()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_first_tag(),
                _ => When_user_clicks_on_second_tag(),
                _ => Then_tag_filter_should_be_appended_to_url(),
                _ => Then_only_endpoints_with_chosen_tags_should_be_shown(),
                _ => Then_should_be_shown_which_tags_are_selected()
                );
        }

        [Scenario]
        public void Applying_url_filter_to_endpoints()
        {
            Runner.RunScenario(
                _ => When_user_navigates_to_home_page_with_filters_in_url(),
                _ => Then_only_endpoints_with_chosen_parameters_should_be_shown()
                );
        }

        [Scenario]
        public void Filters_should_apply_when_traveling_forward_and_backward_on_history()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => With_driver_wait_time(TimeSpan.FromSeconds(1)),
                _ => Given_endpoints_are_visible(),
                _ => When_user_clicks_on_first_tag(),
                _ => When_user_clicks_on_status_button(),
                _ => Then_endpoints_should_be_filtered_with_TAGFILTERS_tag_and_STATUSFILTERS_status_filters(1, 1),
                _ => When_user_navigates_back(),
                _ => Then_endpoints_should_be_filtered_with_TAGFILTERS_tag_and_STATUSFILTERS_status_filters(1, 0),
                _ => When_user_navigates_back(),
                _ => Then_endpoints_should_be_filtered_with_TAGFILTERS_tag_and_STATUSFILTERS_status_filters(0, 0),
                _ => When_user_navigates_forward(),
                _ => Then_endpoints_should_be_filtered_with_TAGFILTERS_tag_and_STATUSFILTERS_status_filters(1, 0),
                _ => When_user_navigates_forward(),
                _ => Then_endpoints_should_be_filtered_with_TAGFILTERS_tag_and_STATUSFILTERS_status_filters(1, 1));
        }
    }
}