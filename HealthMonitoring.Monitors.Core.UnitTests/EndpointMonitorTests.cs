using System;
using System.Linq;
using System.Threading;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class EndpointMonitorTests
    {
        private static readonly TimeSpan TimeComparisonMargin = TimeSpan.FromMilliseconds(300);
        private readonly TestableHealthMonitor _testableHealthMonitor;
        private readonly MonitorableEndpointRegistry _endpointRegistry;

        public EndpointMonitorTests()
        {
            _testableHealthMonitor = new TestableHealthMonitor();
            _endpointRegistry = new MonitorableEndpointRegistry(new HealthMonitorRegistry(new[] { _testableHealthMonitor }));
        }

        [Fact]
        public void Monitor_should_start_checking_the_health_of_endpoints_until_disposed()
        {
            var endpoint1 = _endpointRegistry.TryRegister(CreateEndpointIdentity("address1"));
            _endpointRegistry.TryRegister(CreateEndpointIdentity("address2"));
            _testableHealthMonitor.StartWatch();

            var delay = TimeSpan.FromMilliseconds(400);

            var settings = MonitorSettingsHelper.ConfigureSettings(TimeSpan.FromMilliseconds(50));
            using (new EndpointMonitor(_endpointRegistry, new HealthSampler(settings, new Mock<IEndpointHealthUpdateListener>().Object), settings))
            {
                WaitForAnyCall();

                _endpointRegistry.TryRegister(CreateEndpointIdentity("address3"));
                Thread.Sleep(delay);
                _endpointRegistry.TryUnregister(endpoint1.Identity);
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
        [InlineData(400, 800, 800)]
        [InlineData(800, 400, 800)]
        public void Monitor_should_ping_endpoint_with_regular_intervals(int delayInMs, int intervalInMs, int expectedIntervalInMs)
        {
            _endpointRegistry.TryRegister(CreateEndpointIdentity("address"));
            _testableHealthMonitor.Delay = TimeSpan.FromMilliseconds(delayInMs);
            _testableHealthMonitor.StartWatch();
            var interval = TimeSpan.FromMilliseconds(intervalInMs);

            var settings = MonitorSettingsHelper.ConfigureSettings(interval);

            using (new EndpointMonitor(_endpointRegistry, new HealthSampler(settings, new Mock<IEndpointHealthUpdateListener>().Object), settings))
            {
                WaitForAnyCall();
                Thread.Sleep(TimeSpan.FromMilliseconds(expectedIntervalInMs * 4));
            }

            var intervals = _testableHealthMonitor.Calls.Select(c => c.Item2).ToArray();
            Assert.True(intervals.Length > 1, "There should be more than 1 calls");
            for (int i = 1; i < intervals.Length; ++i)
            {
                var diff = intervals[i] - intervals[i - 1];

                var expected = TimeSpan.FromMilliseconds(expectedIntervalInMs);
                Assert.True((diff - expected).Duration() < TimeComparisonMargin,
                    $"Expected interval {expected.TotalMilliseconds}ms ~ {TimeComparisonMargin.TotalMilliseconds}ms, got {diff.TotalMilliseconds}ms");
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

        private EndpointIdentity CreateEndpointIdentity(string address)
        {
            return new EndpointIdentity(Guid.NewGuid(), _testableHealthMonitor.Name, address);
        }
    }
}