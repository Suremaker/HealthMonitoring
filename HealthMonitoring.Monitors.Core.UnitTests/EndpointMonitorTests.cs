using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Helpers.Time;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers.Awaitable;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class EndpointMonitorTests
    {
        private readonly MonitorableEndpointRegistry _endpointRegistry;
        private readonly Mock<ITimeCoordinator> _mockTimeCoordinator;
        private readonly AwaitableFactory _awaitableFactory = new AwaitableFactory();
        private readonly Mock<IHealthMonitor> _mockHealthMonitor = new Mock<IHealthMonitor>();
        private static readonly TimeSpan TestMaxWaitTime = TimeSpan.FromSeconds(5);

        public EndpointMonitorTests()
        {
            _mockTimeCoordinator = new Mock<ITimeCoordinator>();
            _mockTimeCoordinator.Setup(c => c.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Returns(async () => await Task.Yield());

            _mockHealthMonitor.Setup(cfg => cfg.Name).Returns("test");
            _endpointRegistry = new MonitorableEndpointRegistry(new HealthMonitorRegistry(new[] { _mockHealthMonitor.Object }));
        }

        [Fact]
        public async Task Monitor_should_start_checking_the_health_of_endpoints_until_disposed()
        {
            var endpoint1Countdown = new AsyncCountdown("endpoint1", 2);
            var endpoint2Countdown = new AsyncCountdown("endpoint2", 1);
            var endpoint3Countdown = new AsyncCountdown("endpoint3", 2);
            var counters = new[] { new AsyncCounter(), new AsyncCounter(), new AsyncCounter() };

            _mockHealthMonitor.Setup(cfg => cfg.CheckHealthAsync("address1", It.IsAny<CancellationToken>())).Returns(() => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithCountdown(endpoint1Countdown).WithCounter(counters[0]).RunAsync());
            _mockHealthMonitor.Setup(cfg => cfg.CheckHealthAsync("address2", It.IsAny<CancellationToken>())).Returns(() => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithCountdown(endpoint2Countdown).WithCounter(counters[1]).RunAsync());
            _mockHealthMonitor.Setup(cfg => cfg.CheckHealthAsync("address3", It.IsAny<CancellationToken>())).Returns(() => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithCountdown(endpoint3Countdown).WithCounter(counters[2]).RunAsync());

            var endpoint1 = _endpointRegistry.TryRegister(CreateEndpointIdentity("address1"));
            _endpointRegistry.TryRegister(CreateEndpointIdentity("address2"));

            using (CreateEndpointMonitor(TimeSpan.FromMilliseconds(50)))
            {
                await endpoint1Countdown.WaitAsync(TestMaxWaitTime);
                _endpointRegistry.TryRegister(CreateEndpointIdentity("address3"));
                await endpoint3Countdown.WaitAsync(TestMaxWaitTime);
                _endpointRegistry.TryUnregister(endpoint1.Identity);
                await endpoint2Countdown.ResetTo(50).WaitAsync(TestMaxWaitTime);
            }
            var afterStop = counters.Sum(c => c.Value);
            await Task.Delay(200);
            var afterDelay = counters.Sum(c => c.Value);

            Assert.Equal(afterStop, afterDelay);

            Assert.True(counters[0].Value > 1, $"Expected more than 1 check of address1, got: {counters[0].Value}");
            Assert.True(counters[0].Value < counters[1].Value, $"Expected less checks of address1 than address 2, got: address1={counters[0].Value}, address2={counters[1].Value}");
            Assert.True(counters[2].Value > 1, $"Expected more than 1 check of address3, got: {counters[2].Value}");
        }

        [Fact]
        public async Task Monitor_should_check_endpoint_health_with_regular_intervals()
        {
            var interval = TimeSpan.FromMilliseconds(200);
            var healthCounter = new AsyncCountdown("health", 1);
            var delayCounter = new AsyncCountdown("delay", 1);

            _mockHealthMonitor.Setup(cfg => cfg.CheckHealthAsync("address", It.IsAny<CancellationToken>())).Returns(() => _awaitableFactory.Return(new HealthInfo(HealthStatus.Healthy)).WithTimeline("health").WithCountdown(healthCounter).RunAsync());
            _mockTimeCoordinator.Setup(cfg => cfg.Delay(interval, It.IsAny<CancellationToken>())).Returns(() => _awaitableFactory.Return().WithDelay(interval).WithTimeline("delay").WithCountdown(delayCounter).RunAsync());

            _endpointRegistry.TryRegister(CreateEndpointIdentity("address"));
            using (CreateEndpointMonitor(interval))
                await Task.WhenAll(healthCounter.WaitAsync(TestMaxWaitTime), delayCounter.WaitAsync(TestMaxWaitTime));

            var results = _awaitableFactory.GetTimeline();
            Assert.True(results.Length > 1, "results.Length>1");
            var delayTask = results[0];
            var healthTask = results[1];

            Assert.Equal(delayTask.Tag, "delay");
            Assert.Equal(healthTask.Tag, "health");

            AssertTimeOrder(delayTask.Started, healthTask.Started, healthTask.Finished, delayTask.Finished);
        }

        private EndpointMonitor CreateEndpointMonitor(TimeSpan checkInterval)
        {
            var settings = MonitorSettingsHelper.ConfigureSettings(checkInterval);
            return new EndpointMonitor(_endpointRegistry, new MockSampler(), settings, _mockTimeCoordinator.Object);
        }

        class MockSampler : IHealthSampler
        {
            public async Task<EndpointHealth> CheckHealthAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken)
            {
                await endpoint.Monitor.CheckHealthAsync(endpoint.Identity.Address, cancellationToken);
                return new EndpointHealth(DateTime.MinValue, TimeSpan.Zero, EndpointStatus.Healthy);
            }
        }

        private void AssertTimeOrder(params TimeSpan[] times)
        {
            for (var i = 1; i < times.Length; ++i)
                Assert.True(times[i] > times[i - 1], $"Expected [{i}] {times[i]} > [{i - 1}] {times[i - 1]}");
        }

        private EndpointIdentity CreateEndpointIdentity(string address)
        {
            return new EndpointIdentity(Guid.NewGuid(), _mockHealthMonitor.Object.Name, address);
        }
    }
}