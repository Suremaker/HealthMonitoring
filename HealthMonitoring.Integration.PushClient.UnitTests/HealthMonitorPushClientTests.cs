using System;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Integration.PushClient.UnitTests
{
    public class HealthMonitorPushClientTests
    {
        [Fact]
        public void HealthMonitorPushClient_should_require_not_null_url()
        {
            Assert.Throws<ArgumentNullException>(() => HealthMonitorPushClient.UsingHealthMonitor(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t\n\r ")]
        public void HealthMonitorPushClient_should_allow_empty_url_and_disable_integration(string url)
        {
            var notifier = HealthMonitorPushClient.UsingHealthMonitor(url)
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c").DefineAuthenticationToken("t"))
                .WithHealthCheck(Mock.Of<IHealthChecker>())
                .StartHealthNotifier();

            Assert.Null(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_disable_integration_if_no_client_provided()
        {
            var notifier = new TestablePushClient(null, Mock.Of<ITimeCoordinator>())
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c").DefineAuthenticationToken("t"))
                .WithHealthCheck(Mock.Of<IHealthChecker>())
                .StartHealthNotifier();

            Assert.Null(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_start_notifier_if_client_is_provided()
        {
            var notifier = new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c").DefineAuthenticationToken("t"))
                .WithHealthCheck(Mock.Of<IHealthChecker>())
                .StartHealthNotifier();

            Assert.NotNull(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_require_endpoint_definition_to_start_notifier()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                  .WithHealthCheck(Mock.Of<IHealthChecker>())
                  .StartHealthNotifier());

            Assert.Equal("No endpoint definition provided", ex.Message);
        }

        [Fact]
        public void HealthMonitorPushClient_should_require_health_checker_to_start_notifier()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                  .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c").DefineAuthenticationToken("t"))
                  .StartHealthNotifier());

            Assert.Equal("No health checker provided", ex.Message);
        }
    }
}
