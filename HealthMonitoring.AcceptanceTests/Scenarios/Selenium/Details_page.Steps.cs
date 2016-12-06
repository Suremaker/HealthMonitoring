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
    public partial class Details_page : FeatureFixture, IClassFixture<WebDriverContext>
    {
        #region Private vars

        private readonly string _rowsWithTags =
            $"//table[contains(@class,'endpoints')]//tr/td[6]//span[text() = '{SeleniumHelper.UniqueTags[0]}']//ancestor::tr";
        private readonly string _endpointLinksOfTagRowsOnHomePage;
        private readonly string _endpointGroupsOfTagRowsOnHomePage;
        private readonly string _endpointTagsOnHomePage;

        private const string _endpointNameOnDetailsPage = "/html/body/table[1]/tbody/tr[3]/td[2]";
        private const string _endpointGroupOnDetailsPage = "/html/body/table[1]/tbody/tr[2]/td[2]";
        private const string _endpointTagsDetailsPage = "/html/body/table[1]/tbody/tr[12]/td[2]//span";

        private readonly IWebDriver _driver;
        private readonly RestClient _client;
        private string _endpointName;
        private string _endpointGroup;
        private List<string> _edpointTags;
        private string _detailsPageUrl;
        private readonly string _homePageUrl = $"{SeleniumConfiguration.BaseUrl}?endpoint-frequency=1000000&config-frequency=1000000";
        #endregion

        public Details_page(ITestOutputHelper output, WebDriverContext webDriverContext) : base(output)
        {
            _client = ClientHelper.Build();
            _client.RegisterTestEndpoints();
            _driver = webDriverContext.Driver;

            _endpointLinksOfTagRowsOnHomePage = $"{_rowsWithTags}//td[2]//a";
            _endpointGroupsOfTagRowsOnHomePage = $"{_rowsWithTags}//td[1]";
            _endpointTagsOnHomePage = $"{_rowsWithTags}//td[6]//span";
        }

        public void Given_home_page()
        {
            _driver.LoadUrl(_homePageUrl);
        }

        public void When_user_clicks_on_endpoint_name()
        {
            var detailsLink = _driver.WaitElementsAreRendered(By.XPath(_endpointLinksOfTagRowsOnHomePage)).First();
            _endpointName = detailsLink.Text;
            _endpointGroup = _driver.WaitElementsAreRendered(By.XPath(_endpointGroupsOfTagRowsOnHomePage)).First().Text;
            _edpointTags = GetTagsOfFirstEndpointOnHomePage();

            _detailsPageUrl = detailsLink.GetAttribute("href");
            _driver.LoadUrl(_detailsPageUrl);
        }

        public void Then_name_should_be_the_same_as_on_home_page()
        {
            _driver.WaitUntilPageIsChanged(_detailsPageUrl);

            string nameOnPage = GetEndpointNameOnDetailsPage();
            CustomAssertions.EqualNotStrict(_endpointName, nameOnPage);
        }

        public void Then_group_should_be_the_same_as_on_home_page()
        {
            string groupOnPage = GetEndpointGroupOnDetailsPage();
            CustomAssertions.EqualNotStrict(_endpointGroup, groupOnPage);
        }

        public void Then_tags_should_be_the_same_as_on_home_page()
        {
            var tagsOnPage = GetTagsOnDetailsPage();
            Assert.True(_edpointTags.OrderBy(m => m).SequenceEqual(tagsOnPage.OrderBy(t => t)));
        }

        private string GetEndpointNameOnDetailsPage()
        {
            return _driver.WaitTextIsRendered(By.XPath(_endpointNameOnDetailsPage));
        }

        private string GetEndpointGroupOnDetailsPage()
        {
            return _driver.WaitTextIsRendered(By.XPath(_endpointGroupOnDetailsPage));
        }

        private List<string> GetTagsOfFirstEndpointOnHomePage()
        {
            return _driver.WaitElementsAreRendered(By.XPath(_endpointTagsOnHomePage))
                .Select(m => m.Text)
                .ToList();
        }

        private List<string> GetTagsOnDetailsPage()
        {
            return _driver.FindElements(By.XPath(_endpointTagsDetailsPage))
                .Select(m => m.Text)
                .ToList();
        }
    }
}