using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.SelfHost.Controllers;
using HealthMonitoring.SelfHost.Entities;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Controllers
{
    public class EndpointsControllerTests
    {
        private readonly EndpointsController _controller;
        private readonly Mock<IEndpointRegistry> _endpointRegistry;
        private readonly Mock<IEndpointStatsRepository> _statsRepository;
        public EndpointsControllerTests()
        {
            _endpointRegistry = new Mock<IEndpointRegistry>();
            _statsRepository = new Mock<IEndpointStatsRepository>();
            _controller = new EndpointsController(_endpointRegistry.Object, _statsRepository.Object);
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
            var tags = new[] { "t1", "t2" };

            _endpointRegistry.Setup(r => r.RegisterOrUpdate(monitor, address, group, name, tags)).Returns(id);

            _controller.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9090/");
            var response = _controller.PostRegisterEndpoint(new EndpointRegistration
            {
                Address = address,
                Group = group,
                Name = name,
                MonitorType = monitor,
                Tags = tags
            }) as CreatedNegotiatedContentResult<Guid>;

            Assert.NotNull(response);
            Assert.Equal(id, response.Content);
            Assert.Equal($"http://localhost:9090/api/endpoints/{id}", response.Location.ToString());
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
                .Setup(r => r.RegisterOrUpdate(monitor, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
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
            var endpoint = new Endpoint(new EndpointIdentity(id, "monitor", "address"), new EndpointMetadata("name", "group", new[] { "t1", "t2" }));
            _endpointRegistry.Setup(r => r.GetById(id)).Returns(endpoint);

            var result = _controller.GetEndpoint(id) as OkNegotiatedContentResult<EndpointDetails>;
            Assert.NotNull(result);
            AssertEndpoint(endpoint, result.Content);
            Assert.Equal(EndpointStatus.NotRun, result.Content.Status);
            Assert.Equal(null, result.Content.LastCheckUtc);
            Assert.Equal(null, result.Content.LastResponseTime);
            Assert.Equal(new Dictionary<string, string>(), result.Content.Details);
            Assert.Equal(endpoint.LastModifiedTime, result.Content.LastModifiedTime);
        }

        [Theory]
        [InlineData(EndpointStatus.Healthy)]
        [InlineData(EndpointStatus.Faulty)]
        [InlineData(EndpointStatus.Offline)]
        public void GetEndpoint_should_return_endpoint_information_with_details(EndpointStatus status)
        {
            var id = Guid.NewGuid();

            var endpoint = new Endpoint(new EndpointIdentity(id, "monitor", "address"), new EndpointMetadata("name", "group", new[] { "t1", "t2" }));
            var endpointHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.FromSeconds(5), status, new Dictionary<string, string> { { "abc", "def" } });

            endpoint.UpdateHealth(endpointHealth);
            _endpointRegistry.Setup(r => r.GetById(id)).Returns(endpoint);

            var result = _controller.GetEndpoint(id) as OkNegotiatedContentResult<EndpointDetails>;
            Assert.NotNull(result);
            AssertEndpoint(endpoint, result.Content);

            Assert.Equal(status, result.Content.Status);
            Assert.Equal(endpointHealth.CheckTimeUtc, result.Content.LastCheckUtc);
            Assert.Equal(endpointHealth.ResponseTime, result.Content.LastResponseTime);
            Assert.Equal(endpointHealth.Details, result.Content.Details);
        }

        private static void AssertEndpoint(Endpoint expected, EndpointDetails actual)
        {
            Assert.Equal(expected.Identity.MonitorType, actual.MonitorType);
            Assert.Equal(expected.Identity.Id, actual.Id);
            Assert.Equal(expected.Identity.Address, actual.Address);
            Assert.Equal(expected.Metadata.Name, actual.Name);
            Assert.Equal(expected.Metadata.Group, actual.Group);
        }

        [Fact]
        public void GetEndpoints_should_return_all_endpoints()
        {
            var endpoints = new[]
            {
                new Endpoint(new EndpointIdentity(Guid.NewGuid(),"a", "b"),new EndpointMetadata("c", "d", new[] { "t1", "t2" })),
                new Endpoint(new EndpointIdentity(Guid.NewGuid(), "e", "f"),new EndpointMetadata( "g", "h", new[] { "t1", "t2" }))
            };
            _endpointRegistry.Setup(r => r.Endpoints).Returns(endpoints);
            var results = _controller.GetEndpoints().ToArray();

            foreach (var endpoint in endpoints)
                AssertEndpoint(endpoint, results.SingleOrDefault(r => r.Id == endpoint.Identity.Id));
        }

        [Fact]
        public void GetEndpointStats_should_return_all_stats()
        {
            var endpointId = Guid.NewGuid();
            var endpointStatses = new[]
            {
                new EndpointStats(DateTime.UtcNow, EndpointStatus.Healthy, TimeSpan.FromSeconds(4)),
                new EndpointStats(DateTime.UtcNow.AddMilliseconds(100), EndpointStatus.Faulty, TimeSpan.FromMilliseconds(250))
            };
            var limitDays = 5;
            _statsRepository.Setup(r => r.GetStatistics(endpointId, limitDays)).Returns(endpointStatses);
            var stats = _controller.GetEndpointStats(endpointId, limitDays);
            Assert.Equal(2, stats.Length);
            AssertStats(endpointStatses[0], stats[0]);
            AssertStats(endpointStatses[1], stats[1]);
        }

        [Fact]
        public void GetEndpointStats_should_query_stats_for_one_day_by_default()
        {
            var endpointId = Guid.NewGuid();
            _controller.GetEndpointStats(endpointId, null);
            _statsRepository.Verify(r => r.GetStatistics(endpointId, 1));
        }

        [Fact]
        public void GetEndpointsIdentities_should_return_all_registered_endpoints()
        {
            var endpoints = new[]
            {
                new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor1", "address1"), new EndpointMetadata("name", "group",new string[0])),
                new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor2", "address2"), new EndpointMetadata("name", "group",new string[0])),
                new Endpoint(new EndpointIdentity(Guid.NewGuid(), "monitor3", "address3"), new EndpointMetadata("name", "group",new string[0]))
            };
            _endpointRegistry.Setup(r => r.Endpoints).Returns(endpoints);
            var actual = _controller.GetEndpointsIdentities();
            Assert.Equal(endpoints.Select(e => e.Identity), actual);
        }

        [Fact]
        public void PostEndpointsHealth_should_update_endpoints_health()
        {
            var update1 = new EndpointHealthUpdate { EndpointId = Guid.NewGuid(), CheckTimeUtc = DateTime.UtcNow, Status = EndpointStatus.Offline, ResponseTime = TimeSpan.FromSeconds(5), Details = new Dictionary<string, string> { { "a", "b" } } };
            var update2 = new EndpointHealthUpdate { EndpointId = Guid.NewGuid(), CheckTimeUtc = DateTime.UtcNow, Status = EndpointStatus.Healthy, ResponseTime = TimeSpan.FromSeconds(5), Details = new Dictionary<string, string> { { "a", "b" } } };

            Assert.IsType<OkResult>(_controller.PostEndpointHealth(update1, update2));

            _endpointRegistry.Verify(r => r.UpdateHealth(update1.EndpointId, It.Is<EndpointHealth>(h => AssertHealth(h, update1))));
            _endpointRegistry.Verify(r => r.UpdateHealth(update2.EndpointId, It.Is<EndpointHealth>(h => AssertHealth(h, update2))));
        }

        private static bool AssertHealth(EndpointHealth actual, EndpointHealthUpdate expected)
        {
            Assert.Equal(expected.CheckTimeUtc, actual.CheckTimeUtc);
            Assert.Equal(expected.ResponseTime, actual.ResponseTime);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.Details, actual.Details);
            return true;
        }

        private void AssertStats(EndpointStats expected, EndpointHealthStats actual)
        {
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.CheckTimeUtc, actual.CheckTimeUtc);
            Assert.Equal(expected.ResponseTime, actual.ResponseTime);
        }

        [Fact]
        public void UpdateTags_should_return_BadRequest_if_tags_contains_unallowed_symbols()
        {
            Assert.IsType<BadRequestErrorMessageResult>(_controller.PutUpdateEndpointTags(Guid.NewGuid(), new[] { "tag!@$%^&():,./" }));
        }

        [Fact]
        public void UpdateTags_should_return_NOTFOUND_if_there_is_no_matching_endpoint()
        {
            Assert.IsType<NotFoundResult>(_controller.PutUpdateEndpointTags(Guid.NewGuid(), new[] { "tag" }));
        }
    }
}