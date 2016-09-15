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
    public partial class Dashboard_page : FeatureFixture, IDisposable
    {
        private readonly RestClient _client;
        private readonly IWebDriver _driver;
        private readonly string _healthy = EndpointStatus.Healthy.ToString().ToLower();
        private readonly string _faulty = EndpointStatus.Faulty.ToString().ToLower();
        private readonly string _dashboardUrl = $"{SeleniumConfiguration.BaseUrl}dashboard?endpoint-frequency=1000000&config-frequency=1000000";

        public Dashboard_page(ITestOutputHelper output) : base(output)
        {
            _driver = SeleniumConfiguration.GetWebDriver();
            _client = ClientHelper.Build();
            _client.RegisterTestEndpoints();
        }

        public void Load_dashboard_page()
        {
            _driver.LoadUrl(_dashboardUrl);
        }

        public void Click_on_menu_header_should_redirect_to_home_page()
        {
            // Arrange
            var homeLink = _driver.FindElement(By.XPath("//header//table//*//h1/a"));

            // Act
            homeLink.Click();

            // Assert
            string expectedUrl = SeleniumConfiguration.BaseUrl;
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void Status_filter_should_select_only_healthy_and_faulty_endpoints()
        {
            // Arrange
            var statusMultiselect = _driver.FindElement(By.XPath("//header//*//div[@class='wg-selectBox']"));
            var healthyCheckbox = _driver.FindElement(By.XPath($"//*[@id='wg-selected-{_healthy}']"));
            var faultyCheckbox = _driver.FindElement(By.XPath($"//*[@id='wg-selected-{_faulty}']"));

            // Act
            statusMultiselect.Click();

            healthyCheckbox.Click();
            faultyCheckbox.Click();

            // Assert
            var expectedUrl = $"{_dashboardUrl}&filter-status={_healthy};{_faulty}";

            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
            CheckFilteredStatuses(_faulty, _healthy);
        }

        public void Status_filter_should_be_applied_from_url_parameter()
        {
            // Arrange
            var url = $"{_dashboardUrl}?filter-status={_healthy};{_faulty}";

            // Act
            _driver.Navigate().GoToUrl(url);

            // Assert
            CheckFilteredStatuses(_healthy, _faulty);
        }

        public void Group_view_should_join_endpoints_into_groups()
        {
            var groupViewCheckBox = _driver.FindElement(By.Id("endpointGrouping"));

            groupViewCheckBox.Click();

            // Verify if all groups are distinct
            var groups = GetAllGroupElements();
            var groupNames = GetAllGroupNames();
            Assert.True(groupNames.OrderBy(m => m)
                .SequenceEqual(groupNames.Distinct().OrderBy(m => m)));

            _driver.LoadUrl(groups[0].GetAttribute("href"));

            // Verify if all endpints have same group
            groupNames = GetAllGroupNames();
            Assert.True(groupNames.All(m => m == groupNames[0]));
        }

        public void Group_filter_should_apply_wildcards()
        {
            var wildcardGroup = $"{SeleniumHelper.TestGroups[0].Replace("group", "*")}";
            var groupInput = GetGroupInput();

            groupInput.SendKeys(wildcardGroup);

            var groupNames = GetAllGroupNames();
            var expectedUrl = $"{_dashboardUrl}&filter-group={wildcardGroup}";

            Assert.True(groupNames.All(m => m == SeleniumHelper.TestGroups[0]));
            Assert.True(string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase));
        }

        public void Group_filter_should_apply_wildcards_from_url()
        {
            var wildcardGroup = $"{SeleniumHelper.TestGroups[0].Replace("group", "*")}";
            var url = $"{_dashboardUrl}&filter-group={wildcardGroup}";

            _driver.LoadUrl(url);

            var groupNames = GetAllGroupNames();

            Assert.True(groupNames.All(m => m == SeleniumHelper.TestGroups[0]));
        }

        private void CheckFilteredStatuses(params string[] statuses)
        {
            var filteredStatuses = _driver.FindElements(By.XPath("//div[contains(@class,'board')//a]"));
            Assert.True(filteredStatuses.All(m => statuses.Contains(m.Text, StringComparer.CurrentCultureIgnoreCase)));
        }

        private List<IWebElement> GetAllGroupElements()
        {
            return _driver.FindElements(By.XPath("//div[contains(@class,'board')]//a"))
                .ToList();
        }

        private List<string> GetAllGroupNames()
        {
            return _driver.FindElements(By.XPath("//div[contains(@class,'board')]//a/div[1]"))
                .Select(m => m.Text).ToList();
        }

        private IWebElement GetGroupInput()
        {
            return _driver.FindElement(By.XPath("/html/body/header/table/tbody/tr/td[3]/input[3]"));
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}