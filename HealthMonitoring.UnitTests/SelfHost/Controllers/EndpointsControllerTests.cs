using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web.Http.Results;
using HealthMonitoring.SelfHost.Controllers;
using HealthMonitoring.SelfHost.Entities;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class EndpointsControllerTests
    {
        private readonly EndpointsController _controller;

        public EndpointsControllerTests()
        {
            _controller = new EndpointsController();
        }

        [Theory]
        [InlineData("name", "group", "address", "")]
        [InlineData("name", "group", "", "protocol")]
        [InlineData("name", "", "address", "protocol")]
        [InlineData("", "group", "address", "protocol")]
        public void Registration_should_fail_if_not_all_data_is_provided(string name, string group, string address, string protocol)
        {

            Assert.Throws<ValidationException>(() => _controller.PostRegisterEndpoint(new EndpointRegistration { Address = address, Group = group, Name = name, Protocol = protocol }));
        }

        [Fact]
        public void Registration_should_fail_model_is_not_provided()
        {
            Assert.Throws<ArgumentNullException>(() => _controller.PostRegisterEndpoint(null));
        }

        [Fact]
        public void Registration_should_return_CREATED_status_and_endpoint_identifier()
        {
            _controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9090/");
            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = "abc",
                Group = "def",
                Name = "ghi",
                Protocol = "proto"
            }) as CreatedNegotiatedContentResult<Guid>;

            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.Content);
            Assert.Equal(string.Format("http://localhost:9090/api/endpoints/{0}", response.Content), response.Location.ToString());
        }
    }
}