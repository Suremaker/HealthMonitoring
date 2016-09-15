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
    public partial class Details_page : FeatureFixture, IDisposable
    {
        #region Private vars
        private const string _firstEndpointLinkOnHomePage = "//table[contains(@class,'endpoints')]//tr[2]//td[2]//a";
        private const string _firstEndpointGroupOnHomePage = "//table[contains(@class,'endpoints')]//tr[2]//td[1]";
        private const string _endpointNameOnDetailsPage = "/html/body/table[1]/tbody/tr[3]/td[2]";
        private const string _endpointGroupOnDetailsPage = "/html/body/table[1]/tbody/tr[2]/td[2]";
        private const string _endpointTagsOnHomePage = "//table[contains(@class,'endpoints')]//tr[2]//td[6]//span";
        private const string _endpointTagsDetailsPage = "/html/body/table[1]/tbody/tr[9]/td[2]//span";
        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private string _endpointName;
        private string _endpointGroup;
        private List<string> _edpointTags;
        private readonly string _homePageUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=1000000&config-frequency=1000000";
        #endregion

        public Details_page(ITestOutputHelper output) : base(output)
        {
            _client = ClientHelper.Build();
            _driver = SeleniumConfiguration.GetWebDriver();
            _client.RegisterTestEndpoints();
        }

        public void Load_home_page()
        {
            _driver.LoadUrl(_homePageUrl);
        }

        public void Navigate_to_details_page()
        {
            var detailsLink = _driver.FindElement(By.XPath(_firstEndpointLinkOnHomePage));
            _endpointName = detailsLink.Text;
            _endpointGroup = _driver.FindElement(By.XPath(_firstEndpointGroupOnHomePage)).Text;
            _edpointTags = GetTagsOfFirstEndpointOnHomePage();

            _driver.LoadUrl(detailsLink.GetAttribute("href"));
        }

        public void Details_page_should_display_correct_endpoint_name_end_status()
        {
            var groupOnPage = GetEndpointGroupOnDetailsPage().Text;
            var nameOnPage = GetEndpointNameOnDetailsPage().Text;
            var tagsOnPage = GetTagsOnDetailsPage();

            Assert.True(string.Equals(_endpointGroup, groupOnPage, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(string.Equals(_endpointName, nameOnPage, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(_edpointTags.OrderBy(m => m).SequenceEqual(tagsOnPage.OrderBy(t => t)));
        }

        public IWebElement GetEndpointNameOnDetailsPage()
        {
            return _driver.FindElement(By.XPath(_endpointNameOnDetailsPage));
        }

        public IWebElement GetEndpointGroupOnDetailsPage()
        {
            return _driver.FindElement(By.XPath(_endpointGroupOnDetailsPage));
        }

        public List<string> GetTagsOfFirstEndpointOnHomePage()
        {
            return _driver.FindElements(By.XPath(_endpointTagsOnHomePage))
                .Select(m => m.Text)
                .ToList();
        }

        public List<string> GetTagsOnDetailsPage()
        {
            return _driver.FindElements(By.XPath(_endpointTagsDetailsPage))
                .Select(m => m.Text)
                .ToList();
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}