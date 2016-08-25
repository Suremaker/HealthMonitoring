using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Broken;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using HealthMonitoring.Monitors.Http;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class MonitorDiscoveryTests
    {
        [Fact]
        public void Discovery_should_load_and_instantiate_all_monitors()
        {
            var expected = new[]
            {
                typeof(HttpMonitor), 
                typeof(HttpJsonMonitor), 
                typeof(TestHealthMonitor), 
                typeof(TestHealthMonitor2),
                typeof(TestableHealthMonitor),
            };


            var monitors = MonitorDiscovery.DiscoverAll(
                typeof(HttpMonitor).Assembly.Location,
                typeof(TestHealthMonitor).Assembly.Location);
            CollectionAssert.AreEquivalent(expected, monitors.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_skip_assemblies_that_cannot_be_loaded()
        {
            var expected = new[] { typeof(HttpMonitor), typeof(HttpJsonMonitor) };


            var monitors = MonitorDiscovery.DiscoverAll(
                "some_inexistent_assembly.dll",
                typeof(HttpMonitor).Assembly.Location);

            CollectionAssert.AreEquivalent(expected, monitors.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_monitors_that_cannot_be_instantiated()
        {
            var expected = new[] { typeof(HttpMonitor), typeof(HttpJsonMonitor) };

            var monitors = MonitorDiscovery.DiscoverAll(
                typeof(BrokenMonitor).Assembly.Location,
                typeof(HttpMonitor).Assembly.Location);

            CollectionAssert.AreEquivalent(expected, monitors.Select(p => p.GetType()));
        }

        [Fact]
        public void Discovery_should_scan_given_assembly_only_once()
        {
            var expected = new[] { typeof(HttpMonitor), typeof(HttpJsonMonitor) };

            var monitors = MonitorDiscovery.DiscoverAll(
                typeof(HttpMonitor).Assembly.Location,
                typeof(HttpMonitor).Assembly.Location
                );

            CollectionAssert.AreEquivalent(expected, monitors.Select(p => p.GetType()));
        }
    }

    internal class TestHealthMonitor : IHealthMonitor
    {
        public string Name => "test";

        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class TestHealthMonitor2 : IHealthMonitor
    {
        public string Name => "test";

        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
