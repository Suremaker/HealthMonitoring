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
    public partial class Home_page : FeatureFixture, IDisposable
    {
        private const string _title = "Health Monitoring";
        private const string _filteredStatusElements = "//table[contains(@class,'endpoints')]//tr//td[3]";

        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private readonly string _homeUrl;
        private List<string> _selectedTags;

        public Home_page(ITestOutputHelper output) : base(output)
        {
            _client = ClientHelper.Build();
            _driver = SeleniumConfiguration.GetWebDriver();
            _client.RegisterTestEndpoints();
            _homeUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=1000000&config-frequency=1000000";
        }

        public void Given_home_page()
        {
            _driver.LoadUrl(_homeUrl);
        }

        public void Then_page_should_contain_title()
        {
            Assert.Equal(_driver.Title, _title);
        }

        public void When_user_clicked_on_dashboad_page_link()
        {
            var dashboardLink = _driver.FindElement(By.LinkText("Dashboard"));
            dashboardLink.Click();
        }

        public void Then_dashboard_page_should_be_opened()
        {
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}dashboard";
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void When_user_clicked_on_swagger_page_link()
        {
            var swaggerLink = _driver.FindElement(By.LinkText("API swagger docs"));
            swaggerLink.Click();
        }

        public void Then_swagger_page_should_be_opened()
        {
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}swagger/ui/index";
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void When_user_clicked_on_project_page_link()
        {
            var projectLink = _driver.FindElement(By.LinkText("Project site"));
            projectLink.Click();
        }

        public void Then_project_page_should_be_opened()
        {
            Assert.True(string.Equals(_driver.Url, SeleniumConfiguration.ProjectUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void When_user_clicked_on_status_button()
        {
            var statusElements = GetAllStatusElements();

            // statusElements[0] -> 'Total', so click on next
            statusElements[1].Click();
        }

        public void Then_only_endpoints_with_chosen_status_should_be_shown()
        {
            var selectedStatus = GetSelectedStatusElements().First();
            var filteredStatuses = GetFilteredStatusElements();

            Assert.True(filteredStatuses.All(m => string.Equals(m.Text, selectedStatus.Text, StringComparison.CurrentCultureIgnoreCase)));
            
        }

        public void And_should_be_shown_selected_status()
        {
            var statusElements = GetAllStatusElements();
            var selectedStatus = GetSelectedStatusElements().First();

            Assert.True(string.Equals(selectedStatus.Text, statusElements[1].Text, StringComparison.CurrentCultureIgnoreCase));
        }

        public void And_status_filter_should_be_appended_to_url()
        {
            var selectedStatus = GetSelectedStatusElements().First();
            var expectedUrl = $"{_homeUrl}&filter-status={selectedStatus.Text}";

            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void When_user_clicked_on_endpoints_tag()
        {
            _selectedTags = new List<string>();
            var allTags = GetAllTags();
            var firstTag = allTags.First(m => !string.IsNullOrEmpty(m.Text));

            firstTag.Click();
            _selectedTags.Add(firstTag.Text);

            allTags = GetAllTags();
            var secondTag = allTags.First(m => !string.IsNullOrEmpty(m.Text) && m.Text != _selectedTags.First());

            secondTag.Click();

            _selectedTags.Add(secondTag.Text);
        }

        public void Then_only_endpoints_with_chosen_tags_should_be_shown()
        {
            var filteredTags = GetFilteredTags();
            int filteredEndpointsCount = GetFilteredStatusElements().Count;

            Assert.True(_selectedTags.All(tag => filteredTags.Count(el => el.Text == tag) == filteredEndpointsCount));
        }

        public void And_should_be_shown_selected_tags()
        {
            var selectedTagElements = GetSelectedTags();

            Assert.True(selectedTagElements.All(m => _selectedTags.Any(tag => tag == m.Text)));
        }

        public void And_tag_filter_should_be_appended_to_url()
        {
            string exptectedUrl = $"{_homeUrl}&filter-tags={string.Join(";", _selectedTags)};";

            Assert.True(string.Equals(exptectedUrl, _driver.Url, StringComparison.CurrentCultureIgnoreCase));
        }

        public void When_user_navigates_to_home_page_with_filters()
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

        public void Wait_endpoints_are_rendered()
        {
            var endpointsSelector = By.XPath(_filteredStatusElements);
            _driver.WaitElementsAreRendered(endpointsSelector);
        }

        private List<IWebElement> GetAllTags()
        {
            return _driver.FindElements(By.XPath("//table[contains(@class,'endpoints')]//*//span[contains(@class,'endpointTag')]"))
                .ToList();
        }

        private List<IWebElement> GetSelectedTags()
        {
            return _driver.FindElements(By.XPath("//div[contains(@class, 'selected-filters-container')]//*//span[contains(@class, 'endpointTag')]"))
                .ToList();
        }

        private List<IWebElement> GetFilteredTags()
        {
            return _driver.FindElements(By.XPath("//table[contains(@class,'endpoints')]//tr//td[6]//span"))
                .ToList();
        }

        private List<IWebElement> GetSelectedStatusElements()
        {
            return _driver.FindElements(By.XPath("//div[contains(@class,'selected-filters-container')]//span[not(contains(@class, 'stats-key'))]"))
                .ToList();
        }

        private List<IWebElement> GetAllStatusElements()
        {
            return _driver.FindElements(By.XPath("//*[contains(@class, 'endpoint-status')]"))
                .ToList();
        }
        
        private List<IWebElement> GetFilteredStatusElements()
        {
            return _driver.FindElements(By.XPath(_filteredStatusElements))
                .ToList();
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}