using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TestUtils.Awaitable;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class EndpointMonitorTests
    {
        private const string MonitorType = "test";
        private readonly MonitorableEndpointRegistry _endpointRegistry;
        private readonly Mock<ITimeCoordinator> _mockTimeCoordinator;
        private readonly AwaitableFactory _awaitableFactory = new AwaitableFactory();
        private readonly Mock<IHealthMonitor> _mockHealthMonitor = new Mock<IHealthMonitor>();
        private static readonly TimeSpan TestMaxWaitTime = TimeSpan.FromSeconds(5);
        private readonly Mock<IContinuousTaskExecutor<MonitorableEndpoint>> _mockExecutor;
        private static readonly TimeSpan TestHealthCheckInterval = TimeSpan.FromMilliseconds(157);

        public EndpointMonitorTests()
        {
            _mockTimeCoordinator = new Mock<ITimeCoordinator>();

            _mockHealthMonitor.Setup(cfg => cfg.Name).Returns(MonitorType);
            _endpointRegistry = new MonitorableEndpointRegistry(new HealthMonitorRegistry(new[] { _mockHealthMonitor.Object }));

            _mockExecutor = new Mock<IContinuousTaskExecutor<MonitorableEndpoint>>();
        }

        [Fact]
        public void Monitor_should_add_all_known_endpoints_on_startup()
        {
            var endpoints = Enumerable.Range(0, 10).Select(i => AddNewEndpointToRegistry()).ToArray();

            using (CreateEndpointMonitor()) { }

            foreach (var endpoint in endpoints)
                VerifyTaskWasRegisteredForEndpointInMonitor(endpoint, Times.Once);
        }

        [Fact]
        public void Monitor_should_register_task_for_endpoints_added_during_runtime_but_not_after_it_is_disposed()
        {
            using (CreateEndpointMonitor())
            {
                VerifyTaskWasRegisteredForEndpointInMonitor(AddNewEndpointToRegistry(), Times.Once);
            }

            VerifyTaskWasRegisteredForEndpointInMonitor(AddNewEndpointToRegistry(), Times.Never);
        }

        [Fact]
        public void Monitor_should_dispose_task_executor_on_its_disposal()
        {
            var monitor = CreateEndpointMonitor();
            _mockExecutor.Verify(e => e.Dispose(), Times.Never);
            monitor.Dispose();
            _mockExecutor.Verify(e => e.Dispose(), Times.Once);
        }

        [Fact]
        public async Task The_endpoint_monitoring_should_start_from_randomized_delay_in_order_to_distribute_montior_load_then_check_health_in_regular_intervals()
        {
            var noOfHealthChecks = 3;

            var expectedEventTimeline = new List<string> { "randomDelay_start", "randomDelay_finish" };
            for (int i = 0; i < noOfHealthChecks; ++i)
                expectedEventTimeline.AddRange(new[] { "checkInterval_start", "healthCheck_start", "check_finish", "check_finish" });

            var endpoint = AddNewEndpointToRegistry();
            var capturedTaskFactory = CaptureMonitorTask(endpoint);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                SetupDelayTaskOnTimeLine("randomDelay", requestedDelay => requestedDelay < TestHealthCheckInterval, cancellationTokenSource.Token);
                SetupDelayTaskOnTimeLine("checkInterval", requestedDelay => requestedDelay == TestHealthCheckInterval, cancellationTokenSource.Token);
                SetupEndpointMonitorOnTimeLineToRunNTimes(endpoint, "healthCheck", noOfHealthChecks, cancellationTokenSource.Token);

                await capturedTaskFactory.Invoke(endpoint, cancellationTokenSource.Token);
            }

            var actualEventTimeline = _awaitableFactory.GetOrderedTimelineEvents()
                .Select(eventName => (eventName == "healthCheck_finish" || eventName == "checkInterval_finish") ? "check_finish" : eventName)
                .ToArray();

            Assert.True(expectedEventTimeline.SequenceEqual(actualEventTimeline), $"Expected:\n{string.Join(",", expectedEventTimeline)}\nGot:\n{string.Join(",", actualEventTimeline)}");
        }

        [Fact]
        public async Task The_endpoint_health_check_task_should_be_cancellable()
        {
            var endpoint = AddNewEndpointToRegistry();
            var capturedTaskFactory = CaptureMonitorTask(endpoint);

            var delayNotCancelled = false;
            var healthCheckNotCancelled = false;

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
            {
                var cancellationToken = cancellationTokenSource.Token;
                _mockTimeCoordinator.Setup(c => c.Delay(It.Is<TimeSpan>(ts => ts < TestMaxWaitTime), cancellationToken))
                    .Returns(() => Task.FromResult(0));

                _mockTimeCoordinator
                    .Setup(c => c.Delay(TestHealthCheckInterval, cancellationToken))
                    .Returns(async (TimeSpan delay, CancellationToken token) =>
                    {
                        await Task.Delay(TestMaxWaitTime, token);
                        delayNotCancelled = true;
                    });

                _mockHealthMonitor.Setup(m => m.CheckHealthAsync(endpoint.Identity.Address, cancellationToken)).Returns(
                    async (string address, CancellationToken token) =>
                    {
                        await Task.Delay(TestMaxWaitTime, token);
                        healthCheckNotCancelled = true;
                        endpoint.Dispose();
                        return new HealthInfo(HealthStatus.Healthy);
                    });

                await capturedTaskFactory.Invoke(endpoint, cancellationToken);
            }
            Assert.False(delayNotCancelled, "delayNotCancelled");
            Assert.False(healthCheckNotCancelled, "healthCheckNotCancelled");
        }

        [Theory]
        [InlineData(typeof(Exception))]
        [InlineData(typeof(TaskCanceledException))]
        [InlineData(typeof(OperationCanceledException))]
        public async Task The_endpoint_health_check_should_delay_next_check_execution_after_exception_but_then_retry(Type exceptionType)
        {
            var endpoint = AddNewEndpointToRegistry();
            var capturedTaskFactory = CaptureMonitorTask(endpoint);

            const int totalHealthCheckRuns = 4;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;
                _mockTimeCoordinator.Setup(c => c.Delay(It.IsAny<TimeSpan>(), cancellationToken))
                    .Returns(() => Task.FromResult(0));

                int healthCheckRuns = 0;
                _mockHealthMonitor.Setup(m => m.CheckHealthAsync(endpoint.Identity.Address, cancellationToken))
                    .Returns(() =>
                    {
                        if (healthCheckRuns++ < totalHealthCheckRuns)
                            throw (Exception)Activator.CreateInstance(exceptionType);
                        endpoint.Dispose();
                        return Task.FromResult(new HealthInfo(HealthStatus.Healthy));
                    });

                await capturedTaskFactory.Invoke(endpoint, cancellationToken);

                for (int i = 1; i <= totalHealthCheckRuns; ++i)
                    _mockTimeCoordinator.Verify(c => c.Delay(TimeSpan.FromSeconds(i), cancellationToken), Times.Once());
            }
        }

        private void SetupEndpointMonitorOnTimeLineToRunNTimes(MonitorableEndpoint endpoint, string eventName, int noOfHealthChecks, CancellationToken cancellationToken)
        {
            int repeats = 0;

            _mockHealthMonitor.Setup(m => m.CheckHealthAsync(endpoint.Identity.Address, cancellationToken))
                .Returns(() => _awaitableFactory.Execute(() =>
                {
                    if (++repeats >= noOfHealthChecks)
                        endpoint.Dispose();
                    return new HealthInfo(HealthStatus.Healthy);
                })
                .WithTimeline(eventName).RunAsync());
        }

        private void SetupDelayTaskOnTimeLine(string eventName, Expression<Func<TimeSpan, bool>> delayExpression, CancellationToken cancellationToken)
        {
            _mockTimeCoordinator.Setup(c => c.Delay(It.Is(delayExpression), cancellationToken))
                .Returns(() => _awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(100)).WithTimeline(eventName).RunAsync());
        }

        private MonitorableEndpoint AddNewEndpointToRegistry()
        {
            var id = Guid.NewGuid();
            return _endpointRegistry.TryRegister(new EndpointIdentity(id, MonitorType, "address" + id));
        }

        private void VerifyTaskWasRegisteredForEndpointInMonitor(MonitorableEndpoint endpoint, Func<Times> times)
        {
            _mockExecutor.Verify(e => e.TryRegisterTaskFor(endpoint, It.IsAny<Func<MonitorableEndpoint, CancellationToken, Task>>()), times);
        }

        private EndpointMonitor CreateEndpointMonitor()
        {
            var settings = MonitorSettingsHelper.ConfigureSettings(TestHealthCheckInterval);
            return new EndpointMonitor(_endpointRegistry, new MockSampler(), settings, _mockTimeCoordinator.Object, _mockExecutor.Object);
        }

        private Func<MonitorableEndpoint, CancellationToken, Task> CaptureMonitorTask(MonitorableEndpoint endpoint)
        {
            Func<MonitorableEndpoint, CancellationToken, Task> capturedTaskFactory = null;
            _mockExecutor.Setup(e => e.TryRegisterTaskFor(endpoint, It.IsAny<Func<MonitorableEndpoint, CancellationToken, Task>>()))
                .Returns((MonitorableEndpoint e, Func<MonitorableEndpoint, CancellationToken, Task> taskFactory) =>
                {
                    capturedTaskFactory = taskFactory;
                    return true;
                });

            using (CreateEndpointMonitor()) { }
            Assert.True(capturedTaskFactory != null, "It should capture monitor task");
            return capturedTaskFactory;
        }

        class MockSampler : IHealthSampler
        {
            public async Task<EndpointHealth> CheckHealthAsync(MonitorableEndpoint endpoint, CancellationToken cancellationToken)
            {
                await endpoint.Monitor.CheckHealthAsync(endpoint.Identity.Address, cancellationToken);
                return new EndpointHealth(DateTime.MinValue, TimeSpan.Zero, EndpointStatus.Healthy);
            }
        }
    }
}