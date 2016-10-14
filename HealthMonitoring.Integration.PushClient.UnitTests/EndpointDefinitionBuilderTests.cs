using System;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Integration.PushClient.UnitTests
{
    public class EndpointDefinitionBuilderTests
    {
        private readonly TestablePushClient _client;

        public EndpointDefinitionBuilderTests()
        {
            _client = new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>());
        }

        [Fact]
        public void Definition_builder_should_require_Group_name()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _client.DefineEndpoint(b => b.DefineAddress("a").DefineName("n")));
            Assert.Equal("No endpoint group provided", ex.Message);
        }

        [Fact]
        public void Definition_builder_should_require_endpoint_name()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _client.DefineEndpoint(b => b.DefineAddress("a").DefineGroup("g")));
            Assert.Equal("No endpoint name provided", ex.Message);
        }

        [Fact]
        public void Definition_builder_should_require_address()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _client.DefineEndpoint(b => b.DefineName("n").DefineGroup("g")));
            Assert.Equal("No endpoint address provided", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("\t\n\r ")]
        public void Definition_builder_should_require_non_empty_Group_name(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => _client.DefineEndpoint(b => b.DefineGroup(name)));
            Assert.Equal("Value cannot be empty\r\nParameter name: groupName", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("\t\n\r ")]
        public void Definition_builder_should_require_non_empty_endpoint_Name(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => _client.DefineEndpoint(b => b.DefineName(name)));
            Assert.Equal("Value cannot be empty\r\nParameter name: endpointName", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("\t\n\r ")]
        public void Definition_builder_should_require_non_empty_endpoint_Address(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => _client.DefineEndpoint(b => b.DefineAddress(name)));
            Assert.Equal("Value cannot be empty\r\nParameter name: endpointUniqueName", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("\t\n\r ")]
        public void Definition_builder_should_require_non_empty_endpoint_host_part_of_address(string host)
        {
            var ex = Assert.Throws<ArgumentException>(() => _client.DefineEndpoint(b => b.DefineAddress(host, "uq")));
            Assert.Equal("Value cannot be empty\r\nParameter name: host", ex.Message);
        }
    }
}