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
        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private readonly string _homeUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=1000000&config-frequency=1000000";

        public Home_page(ITestOutputHelper output) : base(output)
        {
            _client = ClientHelper.Build();
            _driver = SeleniumConfiguration.GetWebDriver();
            _client.RegisterTestEndpoints();
        }

        public void Load_home_page()
        {
            _driver.LoadUrl(_homeUrl);
        }

        public void Verify_page_title()
        {
            Assert.Equal(_driver.Title, _title);
        }

        public void Verify_dashboard_link()
        {
            var dashboardLink = _driver.FindElement(By.LinkText("Dashboard"));

            dashboardLink.Click();
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}dashboard";
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void Verify_swagger_link()
        {
            var swaggerLink = _driver.FindElement(By.LinkText("API swagger docs"));

            swaggerLink.Click();
            string expectedUrl = $"{SeleniumConfiguration.BaseUrl}swagger/ui/index";
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void Verify_github_link()
        {
            var githubLink = _driver.FindElement(By.LinkText("Project site"));

            githubLink.Click();
            Assert.True(string.Equals(_driver.Url, SeleniumConfiguration.ProjectUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void After_click_on_status_should_be_displayed_endpoints_with_selected_status()
        {
            // Arrange
            var statusElements = GetAllStatusElements();

            // Act
            // statusElements[0] -> 'Total', so click on next
            statusElements[1].Click();

            // Assert
            var selectedStatus = GetSelectedStatusElements().First();
            var filteredStatuses = GetFilteredStatusElements();
            var expectedUrl = $"{_homeUrl}&filter-status={selectedStatus.Text}";

            Assert.True(string.Equals(selectedStatus.Text, statusElements[1].Text, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(filteredStatuses.All(m => string.Equals(m.Text, selectedStatus.Text, StringComparison.CurrentCultureIgnoreCase)));
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void After_clicking_on_tag_should_be_displayed_endpoints_with_selected_tag()
        {
            // Arrange
            var allTags = GetAllTags();

            var tag = allTags.First(m => !string.IsNullOrEmpty(m.Text));

            // Act
            tag.Click();

            // Assert
            var selectedTags = GetSelectedTags();
            var filteredEndpoints = GetFilteredStatusElements();
            var filteredTags = GetFilteredTags();
            var exptectedUrl = $"{_homeUrl}&filter-tags={tag.Text};";

            Assert.True(selectedTags.Any(m => string.Equals(m.Text, tag.Text)));
            Assert.True(filteredTags.Count(m => m.Text == tag.Text) == filteredEndpoints.Count);
            Assert.True(string.Equals(exptectedUrl, _driver.Url, StringComparison.CurrentCultureIgnoreCase));
        }

        public void Url_filters_should_be_applied_to_endpoints()
        {
            var faulty = "faulty";
            var healthy = "healthy";
            string tag = SeleniumHelper.TestTags[0];
            var url = $"{_homeUrl}&filter-status={faulty};{healthy}&filter-tags={tag}";

            // Act
            _driver.LoadUrl(url);

            // Assert
            var selectedTags = GetSelectedTags();
            var filteredTags = GetFilteredTags();
            var filteredStatuses = GetFilteredStatusElements();

            bool areStatusesCorrect = filteredStatuses.All(m => m.Text.ToLower() == faulty || m.Text.ToLower() == healthy);
            Assert.True(areStatusesCorrect);
            Assert.True(filteredTags.Count(m => m.Text == tag) == filteredStatuses.Count);
            Assert.True(selectedTags.Any(m => string.Equals(m.Text, tag)));
        }

        private List<IWebElement> GetAllTags()
        {
            return _driver.FindElements(By.XPath("//table[contains(@class,'endpoints')]//*//span[contains(@class,'endpointTag')]"))
                .ToList();
        }

        private List<IWebElement> GetSelectedTags()
        {
            return _driver.FindElements(By.XPath("//p[contains(@class,'tagsContainer')]//span[contains(@class, 'endpointTag')]"))
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
            return _driver.FindElements(By.XPath("//table[contains(@class,'endpoints')]//tr//td[3]"))
                .ToList();
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}