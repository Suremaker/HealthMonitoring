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
        #region private vars
        private readonly RestClient _client;
        private readonly IWebDriver _driver;
        private readonly string _healthy = EndpointStatus.Healthy.ToString().ToLower();
        private readonly string _faulty = EndpointStatus.Faulty.ToString().ToLower();
        private string _dashboardUrl;
        private readonly string _groupFilter;

        private const string _endpointTilesSelector = "//div[contains(@class,'board')]//a";
        private const string _endpointGroupNamesSelector = "//div[contains(@class,'board')]//a/div[1]";
        private const string _groupInputSelector = "/html/body/header/table/tbody/tr/td[3]/input[3]";
        #endregion

        public Dashboard_page(ITestOutputHelper output) : base(output)
        {
            _dashboardUrl = $"{SeleniumConfiguration.BaseUrl}dashboard?endpoint-frequency=1000000&config-frequency=1000000";
            _groupFilter = $"{SeleniumHelper.TestGroups[0].Replace("group", "*")}";
            _driver = SeleniumConfiguration.GetWebDriver();
            _client = ClientHelper.Build();
            _client.RegisterTestEndpoints();
        }

        public void Given_dashboard_page()
        {
            _driver.LoadUrl(_dashboardUrl);
        }

        public void When_user_clicked_on_home_link()
        {
            var homeLink = _driver.FindElement(By.XPath("//header//table//*//h1/a"));
            homeLink.Click();
        }

        public void Then_home_page_is_opened()
        {
            string errorMsg = $"actual: {_driver.Url} | expected: {SeleniumConfiguration.BaseUrl}";
            Assert.True(_driver.Url == SeleniumConfiguration.BaseUrl, errorMsg);
        }

        public void When_user_clicked_on_status_select()
        {
            var statusMultiselect = _driver.FindElement(By.XPath("//header//*//div[@class='wg-selectBox']"));
            statusMultiselect.Click();
        }

        public void And_user_selected_healthy_and_faulty_statuses()
        {
            var healthyCheckbox = _driver.FindElement(By.XPath("//*[@id='wg-selected-healthy']"));
            var faultyCheckbox = _driver.FindElement(By.XPath("//*[@id='wg-selected-faulty']"));

            healthyCheckbox.Click();
            faultyCheckbox.Click();
        }

        public void Then_only_healthy_and_faulty_endpoints_should_be_displayed()
        {
            CheckFilteredStatuses(_faulty, _healthy);
        }

        public void And_filter_should_be_displayed_in_page_url()
        {
            var expectedUrl = $"{_dashboardUrl}&filter-status=healthy;faulty";

            Assert.True(_driver.Url == expectedUrl, $"actual: {_driver.Url} | expected: {expectedUrl}");
        }

        public void Given_healthy_and_faulty_statuses_in_url_filter()
        {
            _dashboardUrl = $"{_dashboardUrl}&filter-status=healthy;faulty";
        }

        public void And_load_page()
        {
            _driver.LoadUrl(_dashboardUrl);
        }

        public void When_user_clicked_on_group_view_checkbox()
        {
            var groupViewCheckBox = _driver.FindElement(By.Id("endpointGrouping"));

            groupViewCheckBox.Click();
        }

        public void Then_endpoints_are_grouped()
        {
            // Verify if all groups are distinct
            var groupNames = GetAllGroupNames();
            Assert.True(groupNames.OrderBy(m => m)
                .SequenceEqual(groupNames.Distinct().OrderBy(m => m)));
        }

        public void And_all_endpoints_in_subgroup_have_the_same_group()
        {
            var groups = GetAllGroupElements();

            _driver.LoadUrl(groups[0].GetAttribute("href"));

            var groupNames = GetAllGroupNames();
            Assert.True(groupNames.All(m => m == groupNames[0]));
        }

        public void Given_wildcard_group_filter_in_url()
        {
            _dashboardUrl = $"{_dashboardUrl}&filter-group={_groupFilter}";
        }

        public void When_user_navigates_to_dashboard_page_with_group_filter_in_url()
        {
            _driver.LoadUrl(_dashboardUrl);
        }

        public void Then_are_shown_endpoints_with_group_that_satisfy_filter()
        {
            var groupNames = GetAllGroupNames();
            Assert.True(groupNames.All(m => m == SeleniumHelper.TestGroups[0]));
        }

        public void When_user_writes_group_into_input()
        {
            var groupInput = GetGroupInput();

            groupInput.SendKeys(_groupFilter);
        }

        public void And_group_filter_is_appended_to_url()
        {
            var expectedUrl = $"{_dashboardUrl}&filter-group={_groupFilter}";
            bool urlsAreEqual = string.Equals(_driver.Url, expectedUrl, StringComparison.CurrentCultureIgnoreCase);
            Assert.True(urlsAreEqual, $"actual: {_driver.Url} | expected: {expectedUrl}");
        }

        public void Given_rendered_endpoint_tiles()
        {
            var selector = By.XPath(_endpointTilesSelector);
            _driver.WaitElementsAreRendered(selector);
        }

        private void CheckFilteredStatuses(params string[] statuses)
        {
            var filteredStatuses = GetFilteredStatuses();
            bool selectedCorrectStatuses = filteredStatuses.All(
                m => statuses.Contains(m, StringComparer.CurrentCultureIgnoreCase)
                );

            Assert.True(selectedCorrectStatuses);
        }

        private List<string> GetFilteredStatuses()
        {
            return _driver.FindElements(By.XPath(_endpointTilesSelector))
                .Select(m => m.GetAttribute("data-status"))
                .ToList();
        }

        private List<IWebElement> GetAllGroupElements()
        {
            return _driver.FindElements(By.XPath(_endpointTilesSelector))
                .ToList();
        }

        private List<string> GetAllGroupNames()
        {
            return _driver.FindElements(By.XPath(_endpointGroupNamesSelector))
                .Select(m => m.Text).ToList();
        }

        private IWebElement GetGroupInput()
        {
            return _driver.FindElement(By.XPath(_groupInputSelector));
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}