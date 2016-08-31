using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange;
using HealthMonitoring.Monitors.Core.Exchange.Client;
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
            return CreateExchange(new DataExchangeConfig(1024, 64, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
        }

        private MonitorDataExchange CreateExchange(DataExchangeConfig config)
        {
            return new MonitorDataExchange(_monitorRegistry, _exchangeClient.Object, _endpointRegistry.Object, config);
        }

        [Fact]
        public async Task Exchange_should_try_upload_supported_monitor_types_on_start_until_they_succeeds()
        {
            _exchangeClient.SetupSequence(c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>()
                .Throws<InvalidOperationException>()
                .Returns(Task.FromResult(0));

            SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1))))
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            _exchangeClient.Verify(c => c.RegisterMonitorsAsync(new[] { _healthMonitor.Name }, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Exchange_should_retry_uploading_supported_monitor_types_until_disposal()
        {
            int calls = 0;
            _exchangeClient.Setup(c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    Interlocked.Increment(ref calls);
                    throw new InvalidOperationException();
                });

            SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1))))
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            var capturedCalls = calls;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.Equal(capturedCalls, calls);
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
        public async Task Exchange_should_try_download_list_of_endpoints_periodically()
        {
            SetupDefaultRegisterEndpointsMock();

            var endpointIdentities1 = new[] { new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address1"), new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address2") };
            var endpointIdentities2 = new[] { new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address1"), new EndpointIdentity(Guid.NewGuid(), _healthMonitor.Name, "address3") };

            _exchangeClient
                .SetupSequence(c => c.GetEndpointIdentitiesAsync(It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>()
                .Returns(Task.FromResult(endpointIdentities1))
                .Throws<InvalidOperationException>()
                .Returns(Task.FromResult(endpointIdentities2));

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1))))
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            _endpointRegistry.Verify(r => r.UpdateEndpoints(endpointIdentities1));
            _endpointRegistry.Verify(r => r.UpdateEndpoints(endpointIdentities2));
        }

        [Fact]
        public async Task Exchange_should_try_download_list_of_endpoints_periodically_until_disposal()
        {
            SetupDefaultRegisterEndpointsMock();

            int calls = 0;
            _exchangeClient.Setup(c => c.GetEndpointIdentitiesAsync(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    Interlocked.Increment(ref calls);
                    throw new InvalidOperationException();
                });

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1))))
                await Task.Delay(TimeSpan.FromMilliseconds(500));

            var capturedCalls = calls;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.Equal(capturedCalls, calls);
        }

        [Fact]
        public async Task Exchange_should_upload_monitor_healths_in_buckets()
        {
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            var uploads = new List<int>();
            SetupDefaultHealthUploadMock(x => uploads.Add(x.Length));

            int bucketCount = 4;

            var config = new DataExchangeConfig(100, 10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            var healths = Enumerable.Range(0, config.ExchangeOutBucketSize * bucketCount)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();

            using (var ex = CreateExchange(config))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            Assert.Equal(bucketCount, uploads.Count);
            Assert.Equal(new[] { config.ExchangeOutBucketSize }, uploads.Distinct());
        }

        [Fact]
        public async Task Exchange_should_retry_health_update_send_until_disposal()
        {
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            int calls = 0;
            SetupDefaultHealthUploadMock(x =>
            {
                Interlocked.Increment(ref calls);
                if (calls > 1)
                    throw new InvalidOperationException();
            });

            var healths = Enumerable.Range(0, 5)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();

            using (var ex = CreateExchange(new DataExchangeConfig(100, 1, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1))))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            var capturedCalls = calls;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.Equal(capturedCalls, calls);
        }

        [Fact]
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

            var count = 10;
            var config = new DataExchangeConfig(100, count, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1));

            var healths = Enumerable.Range(0, count)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();

            using (var ex = CreateExchange(config))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await Task.Delay(TimeSpan.FromMilliseconds(500));
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