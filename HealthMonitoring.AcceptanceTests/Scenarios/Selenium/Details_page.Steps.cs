using System;
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
        private const string _firstEndpointLinkOnHomePage = "//table[contains(@class,'endpoints')]//tr[2]//td[2]//a";
        private const string _firstEndpointStatusOnHomePage = "//table[contains(@class,'endpoints')]//tr[2]//td[3]";
        private const string _endpointNameOnDetailsPage = "/html/body/table[1]/tbody/tr[6]/td[2]";
        private const string _endpointStatusOnDetailsPage = "/html/body/table[1]/tbody/tr[3]/td[2]";
        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private string _endpointName;
        private string _endpointStatus;
        private readonly string _homePageUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=10000";

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
            _endpointStatus = _driver.FindElement(By.XPath(_firstEndpointStatusOnHomePage)).Text;

            _driver.LoadUrl(detailsLink.GetAttribute("href"));
        }

        public void Details_page_should_display_correct_endpoint_name_end_status()
        {
            Wait.Until(Timeouts.Default, GetStatusOnDetailsPage, e => e.Displayed, "Endpoint status element is not rendered yet");
            Wait.Until(Timeouts.Default, GetEndpointNameOnDetailsPage, e => e.Displayed, "Endpoint name element is not rendered yet");
            var statusOnPage = GetStatusOnDetailsPage().Text;
            var nameOnPage = GetEndpointNameOnDetailsPage().Text;

            Assert.True(string.Equals(_endpointStatus, statusOnPage, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(string.Equals(_endpointName, nameOnPage, StringComparison.CurrentCultureIgnoreCase));
        }

        public IWebElement GetStatusOnDetailsPage()
        {
            return _driver.FindElement(By.XPath(_endpointNameOnDetailsPage));
        }

        public IWebElement GetEndpointNameOnDetailsPage()
        {
            return _driver.FindElement(By.XPath(_endpointStatusOnDetailsPage));
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}