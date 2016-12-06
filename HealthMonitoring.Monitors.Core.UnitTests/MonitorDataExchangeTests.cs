using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange;
using HealthMonitoring.Monitors.Core.Exchange.Client;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.TestUtils.Awaitable;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class MonitorDataExchangeTests
    {
        private readonly IHealthMonitorRegistry _monitorRegistry;
        private readonly Mock<IHealthMonitorExchangeClient> _exchangeClient;
        private readonly Mock<IMonitorableEndpointRegistry> _endpointRegistry;
        private readonly AwaitableFactory _awaitableFactory;
        private static readonly TimeSpan TestMaxWaitTime = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PostRunDelay = TimeSpan.FromMilliseconds(100);
        private const string MonitorTypeName = "test";

        public MonitorDataExchangeTests()
        {
            _awaitableFactory = new AwaitableFactory();
            _monitorRegistry = new HealthMonitorRegistry(new[] { Mock.Of<IHealthMonitor>(cfg => cfg.Name == MonitorTypeName) });
            _exchangeClient = new Mock<IHealthMonitorExchangeClient>();
            _endpointRegistry = new Mock<IMonitorableEndpointRegistry>();
        }

        private MonitorDataExchange CreateExchange()
        {
            return CreateExchange(new DataExchangeConfig(1024, 64, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag));
        }

        private MonitorDataExchange CreateExchange(DataExchangeConfig config)
        {
            return new MonitorDataExchange(_monitorRegistry, _exchangeClient.Object, _endpointRegistry.Object, config);
        }

        [Fact]
        public async Task Exchange_should_try_upload_supported_monitor_types_on_start_until_they_succeeds()
        {
            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.RegisterMonitorsAsync), 1);

            SetupExchangeClient(
                c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),

                () => { throw new InvalidOperationException(); },
                () => { throw new InvalidOperationException(); },
                () => _awaitableFactory.Return().WithCountdown(countdown).RunAsync()
            );

            SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag)))
                await countdown.WaitAsync(TestMaxWaitTime);
        }

        [Fact]
        public async Task Exchange_should_retry_uploading_supported_monitor_types_until_disposal()
        {
            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.RegisterMonitorsAsync), 10);
            var counter = new AsyncCounter();

            _exchangeClient
                .Setup(c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Throw(new InvalidOperationException()).WithCountdown(countdown).WithCounter(counter).RunAsync());

            SetupDefaultEndpointIdentitiesMock();

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag)))
                await countdown.WaitAsync(TestMaxWaitTime);

            var capturedCalls = counter.Value;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(PostRunDelay);
            Assert.Equal(capturedCalls, counter.Value);
        }

        [Fact]
        public async Task Exchange_should_download_list_of_endpoints()
        {
            SetupDefaultRegisterEndpointsMock();

            var endpointIdentitiesGetCountdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.GetEndpointIdentitiesAsync), 1);

            var ids = SetupDefaultEndpointIdentitiesMock(cfg => cfg.WithCountdown(endpointIdentitiesGetCountdown));

            using (CreateExchange())
                await endpointIdentitiesGetCountdown.WaitAsync(TimeSpan.FromSeconds(5));

            _endpointRegistry.Verify(r => r.UpdateEndpoints(ids));
        }

        [Fact]
        public async Task Exchange_should_try_download_list_of_endpoints_periodically()
        {
            SetupDefaultRegisterEndpointsMock();

            var endpointIdentities1 = new[] { new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address1"), new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address2") };
            var endpointIdentities2 = new[] { new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address1"), new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address3") };
            var countdown1 = new AsyncCountdown("endpointIdentities1", 1);
            var countdown2 = new AsyncCountdown("endpointIdentities2", 1);

            var monitorTag = "some_tag";

            SetupExchangeClient(c => c.GetEndpointIdentitiesAsync(monitorTag, It.IsAny<CancellationToken>()),

                () => { throw new InvalidOperationException(); },
                () => _awaitableFactory.Return(endpointIdentities1).WithCountdown(countdown1).RunAsync(),
                () => { throw new InvalidOperationException(); },
                () => _awaitableFactory.Return(endpointIdentities2).WithCountdown(countdown2).RunAsync());

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1), monitorTag)))
            {
                await countdown1.WaitAsync(TestMaxWaitTime);
                await countdown2.WaitAsync(TestMaxWaitTime);
            }
        }

        [Fact]
        public async Task Exchange_should_try_download_list_of_endpoints_periodically_until_disposal()
        {
            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.RegisterMonitorsAsync), 10);
            var counter = new AsyncCounter();

            SetupDefaultRegisterEndpointsMock();

            _exchangeClient.Setup(c => c.GetEndpointIdentitiesAsync(EndpointMetadata.DefaultMonitorTag, It.IsAny<CancellationToken>()))
                .Returns(() => _awaitableFactory.Throw<EndpointIdentity[]>(new InvalidOperationException()).WithCountdown(countdown).WithCounter(counter).RunAsync());

            using (CreateExchange(new DataExchangeConfig(100, 10, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1), EndpointMetadata.DefaultMonitorTag)))
                await countdown.WaitAsync(TestMaxWaitTime);

            var capturedCalls = counter.Value;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(PostRunDelay);
            Assert.Equal(capturedCalls, counter.Value);
        }

        [Fact]
        public async Task Exchange_should_upload_monitor_healths_in_buckets()
        {
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            int bucketCount = 10;
            int bucketSize = 10;
            var uploads = new List<int>();

            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.UploadHealthAsync), bucketCount);
            SetupDefaultHealthUploadMock(x => { uploads.Add(x.Length); countdown.Decrement(); });

            var healths = PrepareEndpointHealths(bucketSize * bucketCount);

            using (var ex = CreateExchange(new DataExchangeConfig(bucketSize * bucketCount, bucketSize, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag)))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await countdown.WaitAsync(TestMaxWaitTime);
            }

            Assert.Equal(new[] { bucketSize }, uploads.Distinct());
        }

        [Fact]
        public async Task Exchange_should_retry_health_update_send_until_disposal()
        {
            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.UploadHealthAsync), 10);
            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            int calls = 0;
            SetupDefaultHealthUploadMock(x =>
            {
                Interlocked.Increment(ref calls);
                countdown.Decrement();
                if (calls > 1)
                    throw new InvalidOperationException();
            });

            var healths = PrepareEndpointHealths(2);

            using (var ex = CreateExchange(new DataExchangeConfig(100, 1, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag)))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);
                await countdown.WaitAsync(TestMaxWaitTime);
            }

            var capturedCalls = calls;
            Assert.True(capturedCalls > 0, "capturedCalls>0");

            await Task.Delay(PostRunDelay);
            Assert.Equal(capturedCalls, calls);
        }

        [Fact]
        public async Task Exchange_should_retry_health_update_send_in_case_of_errors()
        {
            var count = 10;
            var countdown = new AsyncCountdown(nameof(IHealthMonitorExchangeClient.UploadHealthAsync), count);

            SetupDefaultRegisterEndpointsMock();
            var identity = SetupDefaultEndpointIdentitiesMock().First();

            var attempts = 10;
            SetupDefaultHealthUploadMock(updates =>
            {
                if (attempts-- > 0)
                    throw new Exception();

                foreach (var update in updates)
                    countdown.Decrement();
            });


            var config = new DataExchangeConfig(100, count, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1), EndpointMetadata.DefaultMonitorTag);

            var healths = PrepareEndpointHealths(count);

            using (var ex = CreateExchange(config))
            {
                foreach (var health in healths)
                    ex.UpdateHealth(identity.Id, health);

                await countdown.WaitAsync(TestMaxWaitTime);
            }
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

        private EndpointIdentity[] SetupDefaultEndpointIdentitiesMock(Func<AwaitableBuilder<EndpointIdentity[]>, AwaitableBuilder<EndpointIdentity[]>> mockConfiguration)
        {
            var endpointIdentities = new[] { new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address1"), new EndpointIdentity(Guid.NewGuid(), MonitorTypeName, "address2") };

            _exchangeClient
                .Setup(c => c.GetEndpointIdentitiesAsync(EndpointMetadata.DefaultMonitorTag, It.IsAny<CancellationToken>()))
                .Returns(() => mockConfiguration(_awaitableFactory.Return(endpointIdentities)).RunAsync());
            return endpointIdentities;
        }

        private EndpointIdentity[] SetupDefaultEndpointIdentitiesMock()
        {
            return SetupDefaultEndpointIdentitiesMock(cfg => cfg);
        }

        private void SetupDefaultRegisterEndpointsMock()
        {
            _exchangeClient.Setup(c => c.RegisterMonitorsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
        }

        private void SetupExchangeClient<T>(Expression<Func<IHealthMonitorExchangeClient, T>> clientExpression, params Func<T>[] actions)
        {
            var queue = new Queue<Func<T>>(actions);
            _exchangeClient.Setup(clientExpression).Returns(() => queue.Dequeue().Invoke());
        }

        private static EndpointHealth[] PrepareEndpointHealths(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => new EndpointHealth(DateTime.Today, TimeSpan.FromMilliseconds(i), EndpointStatus.Healthy))
                .ToArray();
        }
    }
}