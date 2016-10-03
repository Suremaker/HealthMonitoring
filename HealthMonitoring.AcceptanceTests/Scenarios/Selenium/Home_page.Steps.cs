using System;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Selenium;
using LightBDD;
using OpenQA.Selenium;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    public partial class Home_page : FeatureFixture, IClassFixture<WebDriverContext>
    {
        private const string _title = "Health Monitoring";
        private const string _filteredStatusElements = "//table[contains(@class,'endpoints')]//tr//td[3]";
        private const string _endpointsGroupsSelector = "//*[@id='main']/article[2]/table/tbody/tr/td[1]";

        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private readonly string _homeUrl;
        private readonly List<string> _selectedTags = new List<string>();

        public Home_page(ITestOutputHelper output, WebDriverContext webDriverContext) : base(output)
        {
            _client = ClientHelper.Build();
            _client.RegisterTestEndpoints();
            _driver = webDriverContext.Driver;
            _homeUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=1000000&config-frequency=1000000";
        }

        public void Given_home_page()
        {
            _driver.LoadUrl(_homeUrl);
        }

        public void Given_endpoints_are_visible()
        {
            _driver.WaitElementsAreRendered(By.XPath(_endpointsGroupsSelector),
                group => group.Displayed && group.Enabled);
        }

        public void Then_page_should_contain_title()
        {
            string actualTitle = Wait.Until(
                Timeouts.Default,
                () => _driver.Title,
                t => !string.IsNullOrEmpty(t),
                "Page does not contain title!");

            CustomAssertions.EqualNotStrict(actualTitle, _title);
        }

        public void When_user_clicks_on_dashboad_page_link()
        {
            var dashboardLink = _driver.WaitElementIsRendered(By.LinkText("Dashboard"));
            dashboardLink.Click();
        }

        public void Then_dashboard_page_should_be_opened()
        {
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}dashboard";
            string actualUrl = _driver.WaitUntilPageIsChanged(expectedUrl);

            CustomAssertions.EqualNotStrict(actualUrl, expectedUrl);
        }

        public void When_user_clicks_on_swagger_page_link()
        {
            var swaggerLink = _driver.WaitElementIsRendered(By.LinkText("API swagger docs"));
            swaggerLink.Click();
        }

        public void Then_swagger_page_should_be_opened()
        {
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}swagger/ui/index";
            string actualUrl = _driver.WaitUntilPageIsChanged(expectedUrl);

            CustomAssertions.EqualNotStrict(actualUrl, expectedUrl);
        }

        public void When_user_clicks_on_project_page_link()
        {
            var projectLink = _driver.WaitElementIsRendered(By.LinkText("Project site"));
            projectLink.Click();
        }

        public void Then_project_page_should_be_opened()
        {
            string expectedUrl = SeleniumConfiguration.ProjectUrl;
            string actualUrl = _driver.WaitUntilPageIsChanged(expectedUrl);

            CustomAssertions.EqualNotStrict(expectedUrl, actualUrl);
        }

        public void When_user_clicks_on_status_button()
        {
            var statusElements = GetAllStatusElements();
            statusElements[1].Click();
        }

        public void Then_only_endpoints_with_chosen_status_should_be_shown()
        {
            var selectedStatus = GetSelectedStatusElements().First();
            var filteredStatuses = GetFilteredStatusElements();

            Assert.True(filteredStatuses.All(m => string.Equals(m.Text, selectedStatus.Text, StringComparison.CurrentCultureIgnoreCase)));
        }

        public void Then_should_be_shown_selected_status()
        {
            var statusElements = GetAllStatusElements();
            var selectedStatus = GetSelectedStatusElements().First();

            CustomAssertions.EqualNotStrict(selectedStatus.Text, statusElements[1].Text);
        }

        public void Then_status_filter_should_be_appended_to_url()
        {
            var selectedStatus = GetSelectedStatusElements().First();
            var expectedUrl = $"{_homeUrl}&filter-status={selectedStatus.Text}";
            var actualUrl = _driver.WaitUntilPageIsChanged(expectedUrl);

            CustomAssertions.EqualNotStrict(actualUrl, expectedUrl);
        }

        public void When_user_clicks_on_first_tag()
        {
            var firstTag = GetAllTags().First();
            _selectedTags.Add(firstTag.Text);
            firstTag.Click();
        }

        public void When_user_clicks_on_second_tag()
        {
            var secondTag = GetAllTags().First(m => m.Text != _selectedTags.First());
            _selectedTags.Add(secondTag.Text);
            secondTag.Click();
        }

        public void Then_only_endpoints_with_chosen_tags_should_be_shown()
        {
            var filteredTags = GetFilteredTags();
            int filteredEndpointsCount = GetFilteredStatusElements().Count;

            Assert.True(_selectedTags.All(tag => filteredTags.Count(el => el.Text == tag) == filteredEndpointsCount));
        }

        public void Then_should_be_shown_which_tags_are_selected()
        {
            var selectedTagElements = GetSelectedTags();

            Assert.True(selectedTagElements.All(m => _selectedTags.Any(tag => tag == m.Text)));
        }

        public void Then_tag_filter_should_be_appended_to_url()
        {
            string exptectedUrl = $"{_homeUrl}&filter-tags={string.Join(";", _selectedTags)};";
            string actualUrl = _driver.WaitUntilPageIsChanged(exptectedUrl);

            CustomAssertions.EqualNotStrict(exptectedUrl, actualUrl);
        }

        public void When_user_navigates_to_home_page_with_filters_in_url()
        {
            var tag = SeleniumHelper.TestTags[0];
            var url = $"{_homeUrl}&filter-status=faulty;healthy&filter-tags={tag}";

            _driver.LoadUrl(url);
        }

        public void Then_only_endpoints_with_chosen_parameters_should_be_shown()
        {
            var tag = SeleniumHelper.TestTags[0];
            var selectedTags = GetSelectedTags();
            var filteredTags = GetFilteredTags();
            var filteredStatuses = GetFilteredStatusElements();

            bool areStatusesCorrect = filteredStatuses.All(
                m => m.Text.ToLower() == "faulty" || 
                m.Text.ToLower() == "healthy");
            Assert.True(areStatusesCorrect);
            Assert.True(filteredTags.Count(m => m.Text == tag) == filteredStatuses.Count);
            Assert.True(selectedTags.Any(m => string.Equals(m.Text, tag)));
        }

        private List<IWebElement> GetAllTags()
        {
            var selector = By.XPath("//table[contains(@class,'endpoints')]//*//span[contains(@class,'endpointTag')]");
            return _driver.WaitElementsAreRendered(selector, elem => !string.IsNullOrEmpty(elem.Text)).ToList();
        }

        private List<IWebElement> GetSelectedTags()
        {
            var selector = By.XPath("//div[contains(@class, 'selected-filters-container')]//*//span[contains(@class, 'endpointTag')]");
            return _driver.WaitElementsAreRendered(selector).ToList();
        }

        private List<IWebElement> GetFilteredTags()
        {
            return _driver.WaitElementsAreRendered(By.XPath("//table[contains(@class,'endpoints')]//tr//td[6]//span")).ToList();
        }

        private List<IWebElement> GetSelectedStatusElements()
        {
            var selector = By.XPath("//div[contains(@class,'selected-filters-container')]//span[not(contains(@class, 'stats-key'))]");
            return _driver.WaitElementsAreRendered(selector).ToList();
        }

        private List<IWebElement> GetAllStatusElements()
        {
            return _driver.WaitElementsAreRendered(By.XPath("//*[contains(@class, 'endpoint-status')]")).ToList();
        }
        
        private List<IWebElement> GetFilteredStatusElements()
        {
            return _driver.WaitElementsAreRendered(By.XPath(_filteredStatusElements)).ToList();
        }
    }
}