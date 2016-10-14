using System;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Helpers;
using HealthMonitoring.Integration.PushClient.UnitTests.Helpers;
using HealthMonitoring.Monitors;
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
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c"))
                .WithHealthCheckMethod(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)))
                .StartHealthNotifier();

            Assert.Null(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_disable_integration_if_no_client_provided()
        {
            var notifier = new TestablePushClient(null, Mock.Of<ITimeCoordinator>())
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c"))
                .WithHealthCheckMethod(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)))
                .StartHealthNotifier();

            Assert.Null(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_start_notifier_if_client_is_provided()
        {
            var notifier = new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c"))
                .WithHealthCheckMethod(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)))
                .StartHealthNotifier();

            Assert.NotNull(notifier);
        }

        [Fact]
        public void HealthMonitorPushClient_should_require_endpoint_definition_to_start_notifier()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                  .WithHealthCheckMethod(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)))
                  .StartHealthNotifier());

            Assert.Equal("No endpoint definition provided", ex.Message);
        }

        [Fact]
        public void HealthMonitorPushClient_should_require_health_check_method_to_start_notifier()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new TestablePushClient(Mock.Of<IHealthMonitorClient>(), Mock.Of<ITimeCoordinator>())
                  .DefineEndpoint(b => b.DefineAddress("a").DefineGroup("b").DefineName("c"))
                  .StartHealthNotifier());

            Assert.Equal("No endpoint health check method provided", ex.Message);
        }
    }
}
