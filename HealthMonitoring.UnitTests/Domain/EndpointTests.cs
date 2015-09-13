using System;
using HealthMonitoring.Model;
using HealthMonitoring.UnitTests.Helpers;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointTests
    {
        [Fact]
        public void Dispose_should_mark_endpoint_disposed()
        {
            var endpoint = new Endpoint(Guid.Empty, ProtocolMock.Mock("proto"), "address", "name", "group");
            Assert.False(endpoint.IsDisposed);
            endpoint.Dispose();
            Assert.True(endpoint.IsDisposed);
        }
    }
}