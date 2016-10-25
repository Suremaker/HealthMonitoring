using System;
using System.Linq;
using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Selenium;
using LightBDD;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios.Selenium
{
    public partial class Swagger_page : FeatureFixture, IClassFixture<WebDriverContext>
    {
        private readonly RestClient _client;
        private readonly IWebDriver _driver;
        private IRestResponse _response;
        private readonly string _swaggerUrl;
        private readonly CredentialsProvider _credentials = new CredentialsProvider();

        private const string _delEndpointUrl = "/api/endpoints/{id}";
        private const string _method = "delete";
        private const string _endpointsSectionSelector ="//li[@id='resource_Endpoints']//a[@class='toggleEndpointList' and text()='Show/Hide']";

        public Swagger_page(ITestOutputHelper output, WebDriverContext webDriverContext) : base(output)
        {
            _client = ClientHelper.Build();
            _client.RegisterTestEndpoints();
            _driver = webDriverContext.Driver;
            _swaggerUrl = $"{SeleniumConfiguration.BaseUrl}/swagger/ui/index";
        }

        public void Given_swagger_page()
        {
            _driver.LoadUrl(_swaggerUrl);
        }

        private void Given_endpoint_with_password_is_registered(string address, string name, string group, string monitor, string password)
        {
            RegisterEndpoint("/api/endpoints/register", name, group, monitor, address, null, password);
        }

        private void Given_endpoint_id_is_received()
        {
            var registrationId = JsonConvert.DeserializeObject<Guid>(_response.Content);
            _credentials.PersonalCredentials.Id = registrationId;
        }

        private void When_user_expand_DELETE_endpoint_section()
        {
            var endpointsSection = _driver.WaitElementIsRendered(By.XPath(_endpointsSectionSelector));
            endpointsSection.Click();

            var selector = GetHeadingUrlSelector(_delEndpointUrl, _method);
            var link = _driver.WaitElementIsRendered(By.XPath(selector));
            link.Click();
        }

        private void When_user_fills_auth_form(Credentials credentials)
        {
            var formSelector = GetAuthFormSelector(_delEndpointUrl, _method);
            var inputs = _driver.WaitElementsAreRendered(By.XPath(formSelector)).ToList();

            inputs.ElementAt(0).SendKeys(credentials.Id.ToString());
            inputs.ElementAt(1).SendKeys(credentials.Password);
        }

        private void When_user_enters_endpoint_id_parameter()
        {
            var selector = GetIdParameterSelector(_delEndpointUrl, _method);
            var parameter = _driver.WaitElementIsRendered(By.XPath(selector));
            parameter.SendKeys(_credentials.PersonalCredentials.Id.ToString());
        }

        private void When_user_sends_request()
        {
            var submitSelector = GetSectionSubmitSelector(_delEndpointUrl, _method);
            var submit = _driver.WaitElementIsRendered(By.XPath(submitSelector));
            submit.Click();
        }

        private void Then_response_status_should_be(HttpStatusCode code)
        {
            var selector = GetResponseStatusSelector(_delEndpointUrl, _method);
            var status = _driver.WaitElementIsRendered(By.XPath(selector));
            var responseCode = ((int) code).ToString();

            Assert.True(string.Equals(status.Text, responseCode));
        }

        private void RegisterEndpoint(
            string url, string name, string group, string monitor,
            string address, string[] tags = null, string password = null,
            Credentials credentials = null)
        {
            object body = new { name, group, monitorType = monitor, address, tags, password };

            _response = _client
                .Authorize(credentials)
                .Post(new RestRequest(url)
                .AddJsonBody(body)
                );
        }

        private string GetApiUrlHeadingSelector(string apiUrl, string method)
        {
            return $"//div[@class='heading' and .//a[text()='{apiUrl}'] and .//a[text()='{method}']]";
        }

        private string GetHeadingUrlSelector(string apiUrl, string method)
        {
            return $"{GetApiUrlHeadingSelector(apiUrl, method)}//a[text()='{apiUrl}']";
        }

        private string GetAuthFormSelector(string apiUrl, string method)
        {
            return $"{GetApiUrlHeadingSelector(apiUrl, method)}//form[@class='auth-form']/input";
        }

        private string GetContentOfSection(string apiUrl, string method)
        {
            return $"{GetApiUrlHeadingSelector(apiUrl, method)}//following-sibling::div[@class='content']";
        }

        private string GetIdParameterSelector(string apiUrl, string method)
        {
            return $"{GetContentOfSection(apiUrl, method)}//tbody[@class='operation-params']//input";
        }

        private string GetSectionSubmitSelector(string apiUrl, string method)
        {
            return $"{GetContentOfSection(apiUrl, method)}//input[@class='submit']";
        }

        private string GetResponseStatusSelector(string apiUrl, string method)
        {
            return $"{GetContentOfSection(apiUrl, method)}//div[contains(@class,'response_code')]";
        }
    }
}
