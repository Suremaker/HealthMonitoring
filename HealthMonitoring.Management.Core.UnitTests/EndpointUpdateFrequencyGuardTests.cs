using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Model;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointUpdateFrequencyGuardTests
    {
        private static readonly TimeSpan TestMaxWaitTime = TimeSpan.FromSeconds(5);
        private readonly Mock<IEndpointRegistry> _endpointRegistry = new Mock<IEndpointRegistry>();
        private readonly Mock<IContinuousTaskExecutor<Endpoint>> _taskExecutor = new Mock<IContinuousTaskExecutor<Endpoint>>();
        private readonly Mock<IMonitorSettings> _monitorSettings = new Mock<IMonitorSettings>();
        private readonly Mock<ITimeCoordinator> _timeCoordinator = new Mock<ITimeCoordinator>();
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _inactivityTimeLimit = TimeSpan.FromSeconds(20);

        public EndpointUpdateFrequencyGuardTests()
        {
            _endpointRegistry.Setup(r => r.Endpoints).Returns(Enumerable.Empty<Endpoint>());
            _monitorSettings.Setup(s => s.HealthCheckInterval).Returns(_checkInterval);
            _monitorSettings.Setup(s => s.HealthUpdateInactivityTimeLimit).Returns(_inactivityTimeLimit);
        }

        [Fact]
        public void Guard_should_add_all_known_endpoints_on_startup()
        {
            var endpoints = Enumerable.Range(0, 10).Select(i => CreateEndpoint()).ToArray();

            _endpointRegistry.Setup(r => r.Endpoints).Returns(endpoints);

            using (CreateGuard()) { }

            foreach (var endpoint in endpoints)
                VerifyTaskWasRegisteredForEndpointInGuard(endpoint, Times.Once);
        }

        [Fact]
        public void Guard_should_register_task_for_endpoints_added_during_runtime_but_not_after_it_is_disposed()
        {
            using (CreateGuard())
                VerifyTaskWasRegisteredForEndpointInGuard(CreateEndpointAndFireEvent(), Times.Once);

            VerifyTaskWasRegisteredForEndpointInGuard(CreateEndpointAndFireEvent(), Times.Never);
        }

        [Fact]
        public void Guard_should_dispose_task_executor_on_its_disposal()
        {
            var monitor = CreateGuard();
            _taskExecutor.Verify(e => e.Dispose(), Times.Never);
            monitor.Dispose();
            _taskExecutor.Verify(e => e.Dispose(), Times.Once);
        }

        [Fact]
        public async Task Guard_should_finish_when_endpoint_is_disposed()
        {
            var endpoint = CreateEndpoint();
            var guardTaskFactory = CaptureGuardTask(endpoint);
            var iterations = 5;

            using (var cancellationTokenSource = new CancellationTokenSource(TestMaxWaitTime))
            {
                SetupEndpointDisposalAfterNthIteration(endpoint, iterations, cancellationTokenSource.Token);
                await guardTaskFactory.Invoke(endpoint, cancellationTokenSource.Token);
            }

            _timeCoordinator.Verify(c => c.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(iterations));
        }

        [Fact]
        public async Task Guard_should_report_endpoint_timeout_if_not_updated_within_specified_time()
        {
            var lastCheckTime = DateTime.UtcNow;
            var currentTime = lastCheckTime + _checkInterval + _inactivityTimeLimit + TimeSpan.FromMilliseconds(1);

            _timeCoordinator.Setup(c => c.UtcNow).Returns(lastCheckTime);
            var endpoint = CreateEndpoint();

            var guardTaskFactory = CaptureGuardTask(endpoint);
            using (var cancellationTokenSource = new CancellationTokenSource(TestMaxWaitTime))
            {
                SetupEndpointDisposalAfterNthIteration(endpoint, 1, cancellationTokenSource.Token);
                _timeCoordinator.Setup(c => c.UtcNow).Returns(currentTime);
                await guardTaskFactory.Invoke(endpoint, cancellationTokenSource.Token);
            }

            _endpointRegistry.Verify(r => r.UpdateHealth(endpoint.Identity.Id, It.Is<EndpointHealth>(h => h.Status == EndpointStatus.TimedOut && h.CheckTimeUtc == currentTime && h.ResponseTime == TimeSpan.Zero)), Times.Once);
        }

        private void SetupEndpointDisposalAfterNthIteration(Endpoint endpoint, int iterations, CancellationToken token)
        {
            _timeCoordinator.Setup(c => c.Delay(It.IsAny<TimeSpan>(), token)).Returns(() =>
            {
                token.ThrowIfCancellationRequested();
                if (--iterations <= 0)
                    endpoint.Dispose();
                return Task.FromResult(0);
            });
        }

        private Endpoint CreateEndpointAndFireEvent()
        {
            var endpoint = CreateEndpoint();
            _endpointRegistry.Raise(r => r.EndpointAdded += null, endpoint);
            return endpoint;
        }

        private Endpoint CreateEndpoint()
        {
            return new Endpoint(_timeCoordinator.Object, new EndpointIdentity(Guid.NewGuid(), string.Empty, string.Empty), new EndpointMetadata(string.Empty, string.Empty, null));
        }

        private EndpointUpdateFrequencyGuard CreateGuard()
        {
            return new EndpointUpdateFrequencyGuard(_endpointRegistry.Object, _taskExecutor.Object, _monitorSettings.Object, _timeCoordinator.Object);
        }

        private void VerifyTaskWasRegisteredForEndpointInGuard(Endpoint endpoint, Func<Times> times)
        {
            _taskExecutor.Verify(e => e.TryRegisterTaskFor(endpoint, It.IsAny<Func<Endpoint, CancellationToken, Task>>()), times);
        }
        private Func<Endpoint, CancellationToken, Task> CaptureGuardTask(Endpoint endpoint)
        {
            _endpointRegistry.Setup(r => r.Endpoints).Returns(Enumerable.Repeat(endpoint, 1));

            Func<Endpoint, CancellationToken, Task> capturedTaskFactory = null;
            _taskExecutor.Setup(e => e.TryRegisterTaskFor(endpoint, It.IsAny<Func<Endpoint, CancellationToken, Task>>()))
                .Returns((Endpoint e, Func<Endpoint, CancellationToken, Task> taskFactory) =>
                {
                    capturedTaskFactory = taskFactory;
                    return true;
                });

            using (CreateGuard()) { }
            Assert.True(capturedTaskFactory != null, "It should capture monitor task");
            return capturedTaskFactory;
        }
    }
}
