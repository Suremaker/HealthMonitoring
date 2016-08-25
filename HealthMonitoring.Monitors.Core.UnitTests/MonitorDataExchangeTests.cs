using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class MonitorDataExchangeTests
    {
        private readonly IHealthMonitorRegistry _monitorRegistry;
        private readonly Mock<IHealthMonitorExchangeClient> _exchangeClient;
        private readonly Mock<IMonitorableEndpointRegistry> _endpointRegistry;
        private readonly TestableHealthMonitor _healthMonitor;

        public MonitorDataExchangeTests()
        {
            _healthMonitor = new TestableHealthMonitor();
            _monitorRegistry = new HealthMonitorRegistry(new[] { _healthMonitor });
            _exchangeClient = new Mock<IHealthMonitorExchangeClient>();
            _endpointRegistry = new Mock<IMonitorableEndpointRegistry>();
        }

        private MonitorDataExchange CreateExchange()
        {
            return new MonitorDataExchange(_monitorRegistry, _exchangeClient.Object, _endpointRegistry.Object);
        }

        [Fact]
        public async Task Exchange_should_upload_supported_monitor_types_on_start()
        {
            SetupDefaultRegisterEndpointsMock();
            SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange())
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            _exchangeClient.Verify(c => c.RegisterMonitorsAsync(new[] { _healthMonitor.Name }, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Exchange_should_download_list_of_endpoints()
        {
            SetupDefaultRegisterEndpointsMock();

            var ids = SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange())
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            _endpointRegistry.Verify(r => r.UpdateEndpoints(ids));
        }

        [Fact]
        public async Task Exchange_should_upload_monitor_healths_in_buckets()
        {
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            var uploads = new List<int>();
            SetupDefaultHealthUploadMock(x => uploads.Add(x.Length));

            int bucketCount = 2;

            var healths = Enumerable.Range(0, MonitorDataExchange.ExchangeOutBucketSize * bucketCount)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();

            using (var ex = CreateExchange())
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            Assert.Equal(bucketCount, uploads.Count);
            Assert.Equal(new[] { MonitorDataExchange.ExchangeOutBucketSize }, uploads.Distinct());
        }

        [Fact]
        //TODO: rework
        public async Task Exchange_should_retry_health_update_send_in_case_of_errors()
        {
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            var uploads = 0;
            var attempts = 1;
            SetupDefaultHealthUploadMock(x =>
            {
                if (attempts-- > 0)
                    throw new Exception();
                uploads += x.Length;
            });

            var count = MonitorDataExchange.ExchangeOutBucketSize;
            var healths = Enumerable.Range(0, count)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();

            using (var ex = CreateExchange())
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await Task.Delay(MonitorDataExchange.UploadRetryInterval.Add(TimeSpan.FromMilliseconds(500)));
            }

            Assert.Equal(count, uploads);
        }

        private void SetupDefaultHealthUploadMock(Action<EndpointHealthUpdate[]> action)
        {
            _exchangeClient.Setup(c => c.UploadHealthAsync(It.IsAny<EndpointHealthUpdate[]>(), It.IsAny<CancellationToken>()))
                .Returns((EndpointHealthUpdate[] u, CancellationToken c) =>
                {
                    action(u);
                    return Task.FromResult(0);
                });
        }

        private EndpointIdentity[] SetupDefaultEndpointIdentitiesMock()
        {
            var endpointIdentities = new[] { new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address1"), new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address2") };

            _exchangeClient
                .Setup(c => c.GetEndpointIdentitiesAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(endpointIdentities));
            return endpointIdentities;
        }

        private void SetupDefaultRegisterEndpointsMock()
        {
            _exchangeClient.Setup(c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
        }
    }
}