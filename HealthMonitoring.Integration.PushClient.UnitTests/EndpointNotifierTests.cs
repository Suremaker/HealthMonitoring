using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Client.Models;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.Monitoring;
using HealthMonitoring.Integration.PushClient.Registration;
using HealthMonitoring.Integration.PushClient.UnitTests.Helpers;
using HealthMonitoring.TestUtils.Awaitable;
using Moq;
using Xunit;

namespace HealthMonitoring.Integration.PushClient.UnitTests
{
    public class EndpointNotifierTests
    {
        private readonly Mock<IHealthMonitorClient> _mockClient = new Mock<IHealthMonitorClient>();
        private readonly AwaitableFactory _awaitableFactory = new AwaitableFactory();
        private static readonly TimeSpan TestMaxTime = TimeSpan.FromSeconds(5);
        private readonly Mock<ITimeCoordinator> _mockTimeCoordinator = new Mock<ITimeCoordinator>();
        private readonly Mock<IBackOffStategy> _mockBackOffStategy = new Mock<IBackOffStategy>();
        private const int MaxEndpointNotifierRetryDelayInSecs = 120;
        private const string AuthenticationToken = "token";

        [Fact]
        public async Task Notifier_should_send_notifications_within_specified_time_span()
        {
            var minimumChecks = 3;
            var expectedEventTimeline = new List<string>();
            for (var i = 0; i < minimumChecks; ++i)
                expectedEventTimeline.AddRange(new[] { "update_start", "update_finish" });

            var countdown = new AsyncCountdown("notifications", minimumChecks);
            var endpointId = Guid.NewGuid();
            var interval = TimeSpan.FromMilliseconds(300);

            SetupHealthCheckInterval(interval);

            SetupEndpointRegistration(endpointId);

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(endpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(100)).WithTimeline("updateHealth").WithCountdown(countdown).RunAsync());

            _mockTimeCoordinator
                .Setup(c => c.Delay(interval, It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(50)).WithTimeline("delay").RunAsync());

            using (CreateNotifier())
                await countdown.WaitAsync(TestMaxTime);

            var actualEventTimeline = GetAggregateFirstAndLastEvents(minimumChecks);

            Assert.True(expectedEventTimeline.SequenceEqual(actualEventTimeline), $"Expected:\n{string.Join(",", expectedEventTimeline)}\nGot:\n{string.Join(",", actualEventTimeline)}");
        }

        private string[] GetAggregateFirstAndLastEvents(int minimumChecks)
        {
            return _awaitableFactory.GetOrderedTimelineEvents()
                .Select(eventName => eventName == "updateHealth_start" || eventName == "delay_start" ? "update_start" : eventName)
                .Select(eventName => eventName == "updateHealth_finish" || eventName == "delay_finish" ? "update_finish" : eventName)
                .Take(minimumChecks * 4)
                .Where((e, i) => i % 4 == 0 || i % 4 == 3)
                .ToArray();
        }

        [Fact]
        public async Task Notifier_should_send_current_health_to_the_API()
        {
            var endpointId = Guid.NewGuid();
            var expectedHealth = new EndpointHealth(HealthStatus.Offline, new Dictionary<string, string> { { "key", "value" } });
            HealthUpdate lastCaptured = null;
            var countdown = new AsyncCountdown("update", 5);

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));
            SetupEndpointRegistration(endpointId);

            _mockClient.Setup(c => c.SendHealthUpdateAsync(endpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, string authToken, HealthUpdate upd, CancellationToken token) => _awaitableFactory
                    .Execute(() => lastCaptured = upd)
                    .WithCountdown(countdown)
                    .RunAsync());

            using (CreateNotifier(token => Task.FromResult(expectedHealth)))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(lastCaptured);
            Assert.Equal(expectedHealth.Status, lastCaptured.Status);
            Assert.Equal(expectedHealth.Details, lastCaptured.Details);
            Assert.True(lastCaptured.CheckTimeUtc > DateTime.UtcNow.AddMinutes(-1) && lastCaptured.CheckTimeUtc < DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public async Task Notifier_should_send_faulty_health_to_the_API_if_provided_health_method_throws()
        {
            var endpointId = Guid.NewGuid();
            HealthUpdate lastCaptured = null;
            var countdown = new AsyncCountdown("update", 5);

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));
            SetupEndpointRegistration(endpointId);

            _mockClient.Setup(c => c.SendHealthUpdateAsync(endpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, string authToken, HealthUpdate upd, CancellationToken token) => _awaitableFactory
                    .Execute(() => lastCaptured = upd)
                    .WithCountdown(countdown)
                    .RunAsync());

            using (CreateNotifier(async token => { await Task.Delay(50, token); throw new InvalidOperationException("some reason"); }))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(lastCaptured);
            Assert.Equal(HealthStatus.Faulty, lastCaptured.Status);
            Assert.Equal("Unable to collect health information", lastCaptured.Details["reason"]);
            Assert.True(lastCaptured.Details["exception"].StartsWith("System.InvalidOperationException: some reason"));
            Assert.True(lastCaptured.CheckTimeUtc > DateTime.UtcNow.AddMinutes(-1) && lastCaptured.CheckTimeUtc < DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public async Task Notifier_should_send_faulty_health_to_the_API_if_provided_health_method_returns_null()
        {
            var endpointId = Guid.NewGuid();
            HealthUpdate lastCaptured = null;
            var countdown = new AsyncCountdown("update", 5);

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));
            SetupEndpointRegistration(endpointId);

            _mockClient.Setup(c => c.SendHealthUpdateAsync(endpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, string authToken, HealthUpdate upd, CancellationToken token) => _awaitableFactory
                    .Execute(() => lastCaptured = upd)
                    .WithCountdown(countdown)
                    .RunAsync());

            using (CreateNotifier(token => Task.FromResult((EndpointHealth)null)))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(lastCaptured);
            Assert.Equal(HealthStatus.Faulty, lastCaptured.Status);
            Assert.Equal("Unable to collect health information", lastCaptured.Details["reason"]);
            Assert.True(lastCaptured.Details["exception"].StartsWith("System.InvalidOperationException: Health information not provided"));
            Assert.True(lastCaptured.CheckTimeUtc > DateTime.UtcNow.AddMinutes(-1) && lastCaptured.CheckTimeUtc < DateTime.UtcNow.AddMinutes(1));
        }

        [Fact]
        public async Task Notifier_should_register_endpoint_with_all_details_and_inferred_host()
        {
            EndpointDefinition captured = null;
            var countdown = new AsyncCountdown("register", 1);
            var endpointId = Guid.NewGuid();

            _mockClient
                .Setup(c => c.RegisterEndpointAsync(It.IsAny<EndpointDefinition>(), It.IsAny<CancellationToken>()))
                .Returns((EndpointDefinition def, CancellationToken token) => _awaitableFactory
                    .Execute(() => { captured = def; return endpointId; })
                    .WithCountdown(countdown)
                    .RunAsync());

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));

            Action<IEndpointDefintionBuilder> builder = b => b.DefineName("endpointName")
                    .DefineGroup("endpointGroup")
                    .DefineTags("t1")
                    .DefineAddress("uniqueName")
                    .DefinePassword(AuthenticationToken);

            using (CreateNotifier(builder, token => Task.FromResult(new EndpointHealth(HealthStatus.Offline))))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(captured);
            Assert.Equal("endpointName", captured.EndpointName);
            Assert.Equal("endpointGroup", captured.GroupName);
            Assert.Equal(new[] { "t1" }, captured.Tags);
            Assert.Equal($"{GetCurrentHost()}:uniqueName", captured.Address);
        }

        [Fact]
        public async Task Notifier_should_register_endpoint_again_if_notification_failed_with_EndpointNotFoundException()
        {
            var oldEndpointId = Guid.NewGuid();
            var newEndpointId = Guid.NewGuid();
            var oldEndpointCountdown = new AsyncCountdown("oldEndpoint", 1);
            var newEndpointCountdown = new AsyncCountdown("newEndpoint", 1);

            _mockClient
                .SetupSequence(c => c.RegisterEndpointAsync(It.IsAny<EndpointDefinition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(oldEndpointId))
                .Returns(Task.FromResult(newEndpointId));

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(oldEndpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory
                    .Throw(new EndpointNotFoundException())
                    .WithCountdown(oldEndpointCountdown)
                    .RunAsync());

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(newEndpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory
                    .Return()
                    .WithCountdown(newEndpointCountdown)
                    .RunAsync());

            using (CreateNotifier())
            {
                await oldEndpointCountdown.WaitAsync(TestMaxTime);
                await newEndpointCountdown.WaitAsync(TestMaxTime);
            }
        }

        [Fact]
        public async Task Notifier_should_register_endpoint_with_all_details_and_specified_host()
        {
            EndpointDefinition captured = null;
            var countdown = new AsyncCountdown("register", 1);
            var endpointId = Guid.NewGuid();

            _mockClient
                .Setup(c => c.RegisterEndpointAsync(It.IsAny<EndpointDefinition>(), It.IsAny<CancellationToken>()))
                .Returns((EndpointDefinition def, CancellationToken token) => _awaitableFactory
                    .Execute(() => { captured = def; return endpointId; })
                    .WithCountdown(countdown)
                    .RunAsync());

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));

            Action<IEndpointDefintionBuilder> builder = b => b.DefineName("endpointName")
                    .DefineGroup("endpointGroup")
                    .DefineTags("t1")
                    .DefineAddress("host", "uniqueName")
                    .DefinePassword(AuthenticationToken);

            using (CreateNotifier(builder, token => Task.FromResult(new EndpointHealth(HealthStatus.Offline))))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(captured);
            Assert.Equal("endpointName", captured.EndpointName);
            Assert.Equal("endpointGroup", captured.GroupName);
            Assert.Equal(new[] { "t1" }, captured.Tags);
            Assert.Equal("host:uniqueName", captured.Address);
        }

        [Fact]
        public async Task Notifier_should_cancel_health_check_on_dispose()
        {
            SetupEndpointRegistration(Guid.NewGuid());
            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));

            var notCancelled = false;
            var countdown = new AsyncCountdown("healthCheck", 1);
            Func<CancellationToken, Task<EndpointHealth>> healthCheck = async token =>
            {
                countdown.Decrement();
                await Task.Delay(TestMaxTime, token);
                notCancelled = true;
                return new EndpointHealth(HealthStatus.Healthy);
            };

            using (CreateNotifier(healthCheck))
                await countdown.WaitAsync(TestMaxTime);

            Assert.False(notCancelled);
        }
        
        [Fact]
        public async Task Notifier_should_retry_sending_health_updates_according_to_recommened_backOff_strategy_in_case_of_exceptions()
        {
            var endpointId = Guid.NewGuid();
            SetupEndpointRegistration(endpointId);
            var checkInterval = TimeSpan.FromMilliseconds(127);

            SetupHealthCheckInterval(checkInterval);
            var minRepeats = 10;
            var countdown = new AsyncCountdown("update", minRepeats);
            var updates = new ConcurrentQueue<HealthUpdate>();

            var backOffPlan = new BackOffPlan(null, false);

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(endpointId, AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, string authToken, HealthUpdate upd, CancellationToken token) =>
                {
                    updates.Enqueue(upd);
                    return _awaitableFactory
                        .Throw(new InvalidOperationException())
                        .WithCountdown(countdown)
                        .RunAsync();
                });
            
            using (CreateNotifier())
                await countdown.WaitAsync(TestMaxTime);

            int expectedSeconds = 1;
            for (int i = 0; i < minRepeats; ++i)
            {
                backOffPlan = new BackOffPlan(TimeSpan.FromSeconds(Math.Min(expectedSeconds, MaxEndpointNotifierRetryDelayInSecs)), true);

                _mockBackOffStategy
                    .Setup(b => b.GetCurrent(TimeSpan.FromSeconds(expectedSeconds), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(backOffPlan));

                _mockTimeCoordinator.Verify(c => c.Delay(TimeSpan.FromSeconds(expectedSeconds), It.IsAny<CancellationToken>()));

                expectedSeconds = Math.Min(expectedSeconds *= 2, MaxEndpointNotifierRetryDelayInSecs);
            }

            _mockTimeCoordinator.Verify(c => c.Delay(checkInterval, It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(1, updates.Distinct().Count());
        }

        [Fact]
        public async Task Notifier_should_survive_communication_errors_and_eventually_restore_connectivity()
        {
            var healthUpdateCountdown = new AsyncCountdown("update", 10);
            var healthUpdateCountdown2 = new AsyncCountdown("update2", 10);
            var registrationCountdown = new AsyncCountdown("registration", 10);
            var intervalCountdown = new AsyncCountdown("interval", 10);
            var delayCountdown = new AsyncCountdown("delay", 10);
            var healthCheckInterval = TimeSpan.FromMilliseconds(127);
            var endpointId = Guid.NewGuid();


            var workingHealthUpdate = _awaitableFactory.Return().WithCountdown(healthUpdateCountdown2);
            var notWorkingHealthUpdate = _awaitableFactory.Throw(new TaskCanceledException()).WithCountdown(healthUpdateCountdown);
            var currentHealthUpdate = notWorkingHealthUpdate;

            var notWorkingRegistration = _awaitableFactory.Throw<Guid>(new TaskCanceledException()).WithCountdown(registrationCountdown);
            var workingRegistration = _awaitableFactory.Return(endpointId);
            var currentRegistration = notWorkingRegistration;

            var notWorkingHealthCheckInterval = _awaitableFactory.Throw<TimeSpan>(new TaskCanceledException()).WithCountdown(intervalCountdown);
            var workingHealthCheckInterval = _awaitableFactory.Return(healthCheckInterval);
            var currentHealthCheckInterval = notWorkingHealthCheckInterval;

            _mockClient.Setup(c => c.SendHealthUpdateAsync(It.IsAny<Guid>(), AuthenticationToken, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => currentHealthUpdate.RunAsync());

            _mockClient.Setup(c => c.RegisterEndpointAsync(It.IsAny<EndpointDefinition>(), It.IsAny<CancellationToken>()))
                .Returns(() => currentRegistration.RunAsync());

            _mockClient.Setup(c => c.GetHealthCheckIntervalAsync(It.IsAny<CancellationToken>()))
                .Returns(() => currentHealthCheckInterval.RunAsync());

            _mockTimeCoordinator.Setup(c => c.Delay(healthCheckInterval, It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Return().WithCountdown(delayCountdown).RunAsync());

            using (CreateNotifier())
            {
                await registrationCountdown.WaitAsync(TestMaxTime);
                currentRegistration = workingRegistration;

                await healthUpdateCountdown.WaitAsync(TestMaxTime);
                currentHealthUpdate = workingHealthUpdate;

                await healthUpdateCountdown2.WaitAsync(TestMaxTime);

                await intervalCountdown.WaitAsync(TestMaxTime);
                currentHealthCheckInterval = workingHealthCheckInterval;

                await delayCountdown.WaitAsync(TestMaxTime);
            }
        }

        private void SetupEndpointRegistration(Guid endpointId)
        {
            _mockClient
                .Setup(c => c.RegisterEndpointAsync(It.IsAny<EndpointDefinition>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(endpointId));
        }

        private void SetupHealthCheckInterval(TimeSpan interval)
        {
            _mockClient
                .Setup(c => c.GetHealthCheckIntervalAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(interval));
        }

        private IEndpointHealthNotifier CreateNotifier()
        {
            return CreateNotifier(token => Task.FromResult(new EndpointHealth(HealthStatus.Healthy)));
        }

        private IEndpointHealthNotifier CreateNotifier(Func<CancellationToken, Task<EndpointHealth>> healthCheck)
        {
            return CreateNotifier(
                b => b.DefineName("name").DefineAddress("address").DefineGroup("group").DefineTags("t1", "t2").DefinePassword(AuthenticationToken),
                healthCheck);
        }

        private IEndpointHealthNotifier CreateNotifier(Action<IEndpointDefintionBuilder> definitionBuilder, Func<CancellationToken, Task<EndpointHealth>> healthCheck)
        {
            var healthChecker = new Mock<IHealthChecker>();
            healthChecker
                .Setup(c => c.CheckHealthAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken token) => healthCheck.Invoke(token));

            return new TestablePushClient(_mockClient.Object, _mockTimeCoordinator.Object)
                 .DefineEndpoint(definitionBuilder)
                 .WithHealthCheck(healthChecker.Object)
                 .StartHealthNotifier();
        }

        private string GetCurrentHost()
        {
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))
            {
                hostName += domainName;
            }

            return hostName;
        }
    }
}