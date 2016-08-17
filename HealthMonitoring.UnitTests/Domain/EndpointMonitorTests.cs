using System;
using System.Linq;
using System.Threading;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointMonitorTests
    {
        private readonly TestableHealthMonitor _testableHealthMonitor;
        private readonly EndpointRegistry _endpointRegistry;

        public EndpointMonitorTests()
        {
            _testableHealthMonitor = new TestableHealthMonitor();
            _endpointRegistry = new EndpointRegistry(new HealthMonitorRegistry(new[] { _testableHealthMonitor }), new Mock<IEndpointConfigurationStore>().Object, new Mock<IEndpointStatsRepository>().Object);
        }

        [Fact]
        public void Monitor_should_start_checking_the_health_of_endpoints_until_disposed()
        {
            var tags = new[] { "t1", "t2" };
            var endpoint1 = _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address1", "group", "name", tags);
            _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address2", "group", "name", tags);
            _testableHealthMonitor.StartWatch();

            var delay = TimeSpan.FromMilliseconds(400);

            var statsManager = new Mock<IEndpointStatsManager>();
            var settings = MonitorSettingsHelper.ConfigureSettings(TimeSpan.FromMilliseconds(50));
            using (new EndpointMonitor(_endpointRegistry, new HealthSampler(settings, statsManager.Object), settings))
            {
                WaitForAnyCall();

                _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address3", "group", "name", tags);
                Thread.Sleep(delay);
                _endpointRegistry.TryUnregisterById(endpoint1);
                Thread.Sleep(delay);
            }
            var afterStop = _testableHealthMonitor.Calls.Count();
            Thread.Sleep(delay);
            var afterDelay = _testableHealthMonitor.Calls.Count();

            Assert.Equal(afterStop, afterDelay);

            var a1 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address1").ToArray();
            var a2 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address2").ToArray();
            var a3 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address3").ToArray();

            Assert.True(a1.Length > 1, $"Expected more than 1 check of address1, got: {a1.Length}");
            Assert.True(a1.Length < a2.Length,
                $"Expected less checks of address1 than address 2, got: address1={a1.Length}, address2={a2.Length}");
            Assert.True(a3.Length > 1, $"Expected more than 1 check of address3, got: {a3.Length}");
        }

        [Theory]
        [InlineData(100, 300, 300)]
        [InlineData(400, 300, 400)]
        public void Monitor_should_ping_endpoint_with_regular_intervals(int delayInMs, int intervalInMs, int expectedIntervalInMs)
        {
            _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address", "group", "name", new[] { "t1", "t2" });
            _testableHealthMonitor.Delay = TimeSpan.FromMilliseconds(delayInMs);
            _testableHealthMonitor.StartWatch();
            var interval = TimeSpan.FromMilliseconds(intervalInMs);

            var settings = MonitorSettingsHelper.ConfigureSettings(interval);

            using (new EndpointMonitor(_endpointRegistry, new HealthSampler(settings, new Mock<IEndpointStatsManager>().Object), settings))
            {
                WaitForAnyCall();
                Thread.Sleep(TimeSpan.FromMilliseconds(expectedIntervalInMs * 5));
            }

            var intervals = _testableHealthMonitor.Calls.Select(c => c.Item2).ToArray();
            Assert.True(intervals.Length > 1, "There should be more than 1 calls");
            for (int i = 1; i < intervals.Length; ++i)
            {
                var diff = intervals[i] - intervals[i - 1];

                var margin = TimeSpan.FromMilliseconds(50);
                var expected = TimeSpan.FromMilliseconds(expectedIntervalInMs);
                Assert.True((diff - expected).Duration() < margin,
                    $"Expected interval {expected.TotalMilliseconds}ms ~ {margin.TotalMilliseconds}ms, got {diff.TotalMilliseconds}ms");
            }
        }

        private void WaitForAnyCall()
        {
            for (int i = 0; i < 10; ++i)
            {
                if (!_testableHealthMonitor.Calls.Any())
                    Thread.Sleep(500);
                else
                    return;
            }
        }
    }
}