using LightBDD;

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
                _ => Then_page_should_contain_title()
                );
        }

        [Scenario]
        public void Verification_of_dashboard_menu_links()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_dashboad_page_link(),
                _ => Then_dashboard_page_should_be_opened()
                );
        }

        [Scenario]
        public void Verification_of_swagger_link()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_swagger_page_link(),
                _ => Then_swagger_page_should_be_opened()
                );
        }

        [Scenario]
        public void Verification_of_project_link()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_project_page_link(),
                _ => Then_project_page_should_be_opened()
                );
        }


        [Scenario]
        public void Status_filter_test()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_status_button(),
                _ => Then_only_endpoints_with_chosen_status_should_be_shown(),
                _ => And_should_be_shown_selected_status(),
                _ => And_status_filter_should_be_appended_to_url()
                );
        }

        [Scenario]
        public void Tag_filter_test()
        {
            Runner.RunScenario(
                _ => Given_home_page(),
                _ => When_user_clicked_on_endpoints_tag(),
                _ => Then_only_endpoints_with_chosen_tags_should_be_shown(),
                _ => And_should_be_shown_selected_tags(),
                _ => And_tag_filter_should_be_appended_to_url()
                );
        }

        [Scenario]
        public void Url_filter_test()
        {
            Runner.RunScenario(
                _ => When_user_navigates_to_home_page_with_filters(),
                _ => Then_only_endpoints_with_chosen_parameters_should_be_shown()
                );
        }
    }
}