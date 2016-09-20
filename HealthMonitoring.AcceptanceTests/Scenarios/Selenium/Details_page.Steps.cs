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

        private readonly string _rowsWithTags =
            $"//table[contains(@class,'endpoints')]//tr/td[6]//span[text() = '{SeleniumHelper.UniqueTags[0]}']//ancestor::tr";
        private readonly string _endpointLinksOfTagRowsOnHomePage;
        private readonly string _endpointGroupsOfTagRowsOnHomePage;
        private readonly string _endpointTagsOnHomePage;

        private const string _endpointNameOnDetailsPage = "/html/body/table[1]/tbody/tr[3]/td[2]";
        private const string _endpointGroupOnDetailsPage = "/html/body/table[1]/tbody/tr[2]/td[2]";
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

            _endpointLinksOfTagRowsOnHomePage = $"{_rowsWithTags}//td[2]//a";
            _endpointGroupsOfTagRowsOnHomePage = $"{_rowsWithTags}//td[1]";
            _endpointTagsOnHomePage = $"{_rowsWithTags}//td[6]//span";
        }

        public void Given_home_page()
        {
            _driver.LoadUrl(_homePageUrl);
        }

        public void When_user_clicked_on_endpoint_name()
        {
            var detailsLink = _driver.FindElements(By.XPath(_endpointLinksOfTagRowsOnHomePage)).First();
            _endpointName = detailsLink.Text;
            _endpointGroup = _driver.FindElements(By.XPath(_endpointGroupsOfTagRowsOnHomePage)).First().Text;
            _edpointTags = GetTagsOfFirstEndpointOnHomePage();

            _driver.LoadUrl(detailsLink.GetAttribute("href"));
        }

        public void Then_name_and_group_should_be_the_same_as_on_home_page()
        {
            var groupOnPage = GetEndpointGroupOnDetailsPage().Text;
            var nameOnPage = GetEndpointNameOnDetailsPage().Text;
            var tagsOnPage = GetTagsOnDetailsPage();

            Assert.True(string.Equals(_endpointGroup, groupOnPage, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(string.Equals(_endpointName, nameOnPage, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(_edpointTags.OrderBy(m => m).SequenceEqual(tagsOnPage.OrderBy(t => t)));
        }

        public void And_elements_are_rendered()
        {
            var nameSelector = By.XPath(_endpointNameOnDetailsPage);
            _driver.WaitElementIsRendered(nameSelector);
            var groupSelector = By.XPath(_endpointGroupOnDetailsPage);
            _driver.WaitElementIsRendered(groupSelector);
            var tagSelector = By.XPath(_endpointTagsDetailsPage);
            _driver.WaitElementsAreRendered(tagSelector);
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