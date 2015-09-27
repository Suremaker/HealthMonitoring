using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors;
using HealthMonitoring.SelfHost.Controllers;
using HealthMonitoring.SelfHost.Entities;
using HealthMonitoring.UnitTests.Helpers;
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
        [InlineData("name", "group", "", "monitor")]
        [InlineData("name", "", "address", "monitor")]
        [InlineData("", "group", "address", "monitor")]
        public void RegisterOrUpdate_should_fail_if_not_all_data_is_provided(string name, string group, string address, string monitor)
        {
            Assert.Throws<ValidationException>(() => _controller.PostRegisterEndpoint(new EndpointRegistration { Address = address, Group = group, Name = name, MonitorType = monitor }));
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
            var monitor = "monitor";
            var address = "abc";
            var group = "def";
            var name = "ghi";
            _endpointRegistry.Setup(r => r.RegisterOrUpdate(monitor, address, group, name)).Returns(id);

            _controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9090/");
            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = address,
                Group = group,
                Name = name,
                MonitorType = monitor
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
        public void DeleteEndpoint_should_return_NOTFOUND_if_there_is_no_matching_endpoint()
        {
            Assert.IsType<NotFoundResult>(_controller.DeleteEndpoint(Guid.NewGuid()));
        }

        [Fact]
        public void DeleteEndpoint_should_return_OK_if_there_is_matching_endpoint()
        {
            var id = Guid.NewGuid();
            _endpointRegistry.Setup(r => r.TryUnregisterById(id)).Returns(true);
            Assert.IsType<OkResult>(_controller.DeleteEndpoint(id));
        }

        [Fact]
        public void RegisterOrUpdate_should_fail_if_monitor_is_not_recognized()
        {
            var monitor = "monitor";
            _endpointRegistry
                .Setup(r => r.RegisterOrUpdate(monitor, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new UnsupportedMonitorException(monitor));

            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = "address",
                Group = "group",
                Name = "name",
                MonitorType = monitor
            }) as BadRequestErrorMessageResult;

            Assert.NotNull(response);
            Assert.Equal("Unsupported monitor: monitor", response.Message);
        }

        [Fact]
        public void GetEndpoint_should_return_endpoint_information()
        {
            Guid id = Guid.NewGuid();
            var endpoint = new Endpoint(id, MonitorMock.Mock("monitor"), "address", "name", "group");
            _endpointRegistry.Setup(r => r.GetById(id)).Returns(endpoint);

            var result = _controller.GetEndpoint(id) as OkNegotiatedContentResult<EndpointDetails>;
            Assert.NotNull(result);
            AssertEndpoint(endpoint, result.Content);
            Assert.Equal(EndpointStatus.NotRun, result.Content.Status);
            Assert.Equal(null, result.Content.LastCheckUtc);
            Assert.Equal(null, result.Content.LastResponseTime);
            Assert.Equal(new Dictionary<string, string>(), result.Content.Details);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Faulty)]
        [InlineData(HealthStatus.Offline)]
        public void GetEndpoint_should_return_endpoint_information_with_details(HealthStatus status)
        {
            Guid id = Guid.NewGuid();
            var monitor = MonitorMock.GetMock("monitor");
            var healthInfo = new HealthInfo(status, TimeSpan.FromSeconds(2), new Dictionary<string, string> { { "a", "b" }, { "c", "d" } });
            monitor.Setup(p => p.CheckHealthAsync("address", It.IsAny<CancellationToken>())).Returns(Task.FromResult(healthInfo));

            var endpoint = new Endpoint(id, monitor.Object, "address", "name", "group");
            endpoint.CheckHealth(new CancellationToken(), MonitorSettingsHelper.ConfigureDefaultSettings()).Wait();
            _endpointRegistry.Setup(r => r.GetById(id)).Returns(endpoint);

            var result = _controller.GetEndpoint(id) as OkNegotiatedContentResult<EndpointDetails>;
            Assert.NotNull(result);
            AssertEndpoint(endpoint, result.Content);

            Assert.Equal(status.ToString(), result.Content.Status.ToString());
            Assert.NotNull(result.Content.LastCheckUtc);
            Assert.Equal(healthInfo.ResponseTime, result.Content.LastResponseTime);
            Assert.Equal(healthInfo.Details, result.Content.Details);
        }

        private static void AssertEndpoint(Endpoint expected, EndpointDetails actual)
        {
            Assert.Equal(expected.MonitorType, actual.MonitorType);
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
                new Endpoint(Guid.NewGuid(), MonitorMock.Mock("a"), "b", "c", "d"),
                new Endpoint(Guid.NewGuid(), MonitorMock.Mock("e"), "f", "g", "h")
            };
            _endpointRegistry.Setup(r => r.Endpoints).Returns(endpoints);
            var results = _controller.GetEndpoints().ToArray();

            foreach (var endpoint in endpoints)
                AssertEndpoint(endpoint, results.SingleOrDefault(r => r.Id == endpoint.Id));
        }
    }
}