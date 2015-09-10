using System.Linq;
using HealthMonitoring.Protocols;
using HealthMonitoring.Protocols.Broken;
using HealthMonitoring.Protocols.Rest;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class ProtocolDiscoveryTests
    {
        [Fact]
        public void Discovery_should_load_and_instantiate_all_protocols()
        {
            var expected = new[]
            {
                typeof(RestProtocol), 
                typeof(TestProtocol), 
                typeof(TestProtocol2)
            };


            var protocols = ProtocolDiscovery.DiscoverAll(
                typeof(RestProtocol).Assembly.Location,
                typeof(TestProtocol).Assembly.Location);

            Assert.Equal(expected, protocols.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_skip_assemblies_that_cannot_be_loaded()
        {
            var expected = new[] { typeof(RestProtocol) };


            var protocols = ProtocolDiscovery.DiscoverAll(
                "some_inexistent_assembly.dll",
                typeof(RestProtocol).Assembly.Location);

            Assert.Equal(expected, protocols.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_protocols_that_cannot_be_instantiated()
        {
            var expected = new[] { typeof(RestProtocol) };


            var protocols = ProtocolDiscovery.DiscoverAll(
                typeof(BrokenProtocol).Assembly.Location,
                typeof(RestProtocol).Assembly.Location);

            Assert.Equal(expected, protocols.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_scan_given_assembly_only_once()
        {
            var expected = new[] { typeof(RestProtocol) };


            var protocols = ProtocolDiscovery.DiscoverAll(
                typeof(RestProtocol).Assembly.Location,
                typeof(RestProtocol).Assembly.Location
                );

            Assert.Equal(expected, protocols.Select(p => p.GetType()));
        }
    }

    internal class TestProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "test"; } }
        
    }

    internal class TestProtocol2 : IHealthCheckProtocol
    {
        public string Name
        {
            get { return "test"; }
        }

    }
}
