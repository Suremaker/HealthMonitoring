using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using HealthMonitoring.Model;
using HealthMonitoring.SelfHost.Controllers;
using HealthMonitoring.SelfHost.Entities;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.SelfHost.Controllers
{
    public class EndpointsControllerTests
    {
        private readonly EndpointsController _controller;
        private readonly Mock<IEndpointRegistry> _endpointRegistry;

        public EndpointsControllerTests()
        {
            _endpointRegistry = new Mock<IEndpointRegistry>();
            _controller = new EndpointsController(_endpointRegistry.Object);
        }

        [Theory]
        [InlineData("name", "group", "address", "")]
        [InlineData("name", "group", "", "protocol")]
        [InlineData("name", "", "address", "protocol")]
        [InlineData("", "group", "address", "protocol")]
        public void RegisterOrUpdate_should_fail_if_not_all_data_is_provided(string name, string group, string address, string protocol)
        {
            Assert.Throws<ValidationException>(() => _controller.PostRegisterEndpoint(new EndpointRegistration { Address = address, Group = group, Name = name, Protocol = protocol }));
        }

        [Fact]
        public void RegisterOrUpdate_should_fail_if_model_is_not_provided()
        {
            Assert.Throws<ArgumentNullException>(() => _controller.PostRegisterEndpoint(null));
        }

        [Fact]
        public void RegisterOrUpdate_should_return_CREATED_status_and_endpoint_identifier()
        {
            Guid id = Guid.NewGuid();
            var protocol = "proto";
            var address = "abc";
            var group = "def";
            var name = "ghi";
            _endpointRegistry.Setup(r => r.RegisterOrUpdate(protocol, address, group, name)).Returns(id);

            _controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9090/");
            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = address,
                Group = group,
                Name = name,
                Protocol = protocol
            }) as CreatedNegotiatedContentResult<Guid>;

            Assert.NotNull(response);
            Assert.Equal(id, response.Content);
            Assert.Equal(string.Format("http://localhost:9090/api/endpoints/{0}", id), response.Location.ToString());
        }

        [Fact]
        public void GetEndpoint_should_return_NOTFOUND_if_there_is_no_matching_endpoint()
        {
            Assert.IsType<NotFoundResult>(_controller.GetEndpoint(Guid.NewGuid()));
        }

        [Fact]
        public void RegisterOrUpdate_should_fail_if_protocol_is_not_recognized()
        {
            var protocol = "proto";
            _endpointRegistry
                .Setup(r => r.RegisterOrUpdate(protocol, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new UnsupportedProtocolException(protocol));

            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = "address",
                Group = "group",
                Name = "name",
                Protocol = protocol
            }) as BadRequestErrorMessageResult;

            Assert.NotNull(response);
            Assert.Equal("Unsupported protocol: proto", response.Message);
        }

        [Fact]
        public void GetEndpoint_should_return_endpoint_information()
        {
            Guid id = Guid.NewGuid();
            var endpoint = new Endpoint(id, "proto", "address", "name", "group");
            _endpointRegistry.Setup(r => r.GetById(id)).Returns(endpoint);

            var result = _controller.GetEndpoint(id) as OkNegotiatedContentResult<EndpointDetails>;
            Assert.NotNull(result);
            AssertEndpoint(endpoint, result.Content);
        }

        private static void AssertEndpoint(Endpoint expected, EndpointDetails actual)
        {
            Assert.Equal(expected.Protocol, actual.Protocol);
            Assert.Equal(expected.Address, actual.Address);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Group, actual.Group);
            Assert.Equal(expected.Id, actual.Id);
        }

        [Fact]
        public void GetEndpoints_should_return_all_endpoints()
        {
            var endpoints = new[]
            {
                new Endpoint(Guid.NewGuid(), "a", "b", "c", "d"),
                new Endpoint(Guid.NewGuid(), "e", "f", "g", "h")
            };
            _endpointRegistry.Setup(r => r.Endpoints).Returns(endpoints);
            var results = _controller.GetEndpoints().ToArray();

            foreach (var endpoint in endpoints)
                AssertEndpoint(endpoint, results.SingleOrDefault(r => r.Id == endpoint.Id));
        }
    }
}