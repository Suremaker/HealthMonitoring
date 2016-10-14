using System;
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
using HealthMonitoring.Monitors;
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

        [Fact]
        public async Task Notifier_should_send_notifications_within_specified_time_span()
        {
            var minimumChecks = 3;
            var expectedEventTimeline = new List<string>();
            for (var i = 0; i < minimumChecks; ++i)
                expectedEventTimeline.AddRange(new[] { "updateHealth_start", "delay_start", "update_finish", "update_finish" });

            var countdown = new AsyncCountdown("notifications", minimumChecks);
            var endpointId = Guid.NewGuid();
            var interval = TimeSpan.FromMilliseconds(300);

            SetupHealthCheckInterval(interval);

            SetupEndpointRegistration(endpointId);

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(endpointId, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(50)).WithTimeline("updateHealth").WithCountdown(countdown).RunAsync());

            _mockTimeCoordinator
                .Setup(c => c.Delay(interval, It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(50)).WithTimeline("delay").RunAsync());

            using (CreateNotifier())
                await countdown.WaitAsync(TestMaxTime);

            var actualEventTimeline = _awaitableFactory.GetOrderedTimelineEvents()
                .Select(eventName => (eventName == "updateHealth_finish" || eventName == "delay_finish") ? "update_finish" : eventName)
                .Take(minimumChecks * 4)
                .ToArray();

            Assert.True(expectedEventTimeline.SequenceEqual(actualEventTimeline), $"Expected:\n{string.Join(",", expectedEventTimeline)}\nGot:\n{string.Join(",", actualEventTimeline)}");
        }

        [Fact]
        public async Task Notifier_should_send_current_health_to_the_API()
        {
            var endpointId = Guid.NewGuid();
            var expectedHealth = new HealthInfo(HealthStatus.Offline, new Dictionary<string, string> { { "key", "value" } });
            HealthUpdate lastCaptured = null;
            var countdown = new AsyncCountdown("update", 5);

            SetupHealthCheckInterval(TimeSpan.FromMilliseconds(1));
            SetupEndpointRegistration(endpointId);

            _mockClient.Setup(c => c.SendHealthUpdateAsync(endpointId, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, HealthUpdate upd, CancellationToken token) => _awaitableFactory
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

            _mockClient.Setup(c => c.SendHealthUpdateAsync(endpointId, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns((Guid id, HealthUpdate upd, CancellationToken token) => _awaitableFactory
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
                    .DefineTags(new[] { "t1" })
                    .DefineAddress("uniqueName");

            using (CreateNotifier(builder, token => Task.FromResult(new HealthInfo(HealthStatus.NotExists))))
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
                .Setup(c => c.SendHealthUpdateAsync(oldEndpointId, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory
                    .Throw(new EndpointNotFoundException())
                    .WithCountdown(oldEndpointCountdown)
                    .RunAsync());

            _mockClient
                .Setup(c => c.SendHealthUpdateAsync(newEndpointId, It.IsAny<HealthUpdate>(), It.IsAny<CancellationToken>()))
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
                    .DefineTags(new[] { "t1" })
                    .DefineAddress("host", "uniqueName");

            using (CreateNotifier(builder, token => Task.FromResult(new HealthInfo(HealthStatus.NotExists))))
                await countdown.WaitAsync(TestMaxTime);

            Assert.NotNull(captured);
            Assert.Equal("endpointName", captured.EndpointName);
            Assert.Equal("endpointGroup", captured.GroupName);
            Assert.Equal(new[] { "t1" }, captured.Tags);
            Assert.Equal("host:uniqueName", captured.Address);
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
            return CreateNotifier(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)));
        }

        private IEndpointHealthNotifier CreateNotifier(Func<CancellationToken, Task<HealthInfo>> healthCheck)
        {
            return CreateNotifier(
                b => b.DefineName("name").DefineAddress("address").DefineGroup("group").DefineTags(new[] { "t1", "t2" }),
                healthCheck);
        }

        private IEndpointHealthNotifier CreateNotifier(Action<IEndpointDefintionBuilder> definitionBuilder, Func<CancellationToken, Task<HealthInfo>> healthCheck)
        {
            return new TestablePushClient(_mockClient.Object, _mockTimeCoordinator.Object)
                 .DefineEndpoint(definitionBuilder)
                 .WithHealthCheckMethod(healthCheck)
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