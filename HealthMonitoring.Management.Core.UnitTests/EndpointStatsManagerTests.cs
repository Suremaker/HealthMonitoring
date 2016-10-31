using System;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.TestUtils;
using HealthMonitoring.TestUtils.Awaitable;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointStatsManagerTests
    {
        private static readonly TimeSpan MaxTestTime = TimeSpan.FromSeconds(5);

        [Fact]
        public void RecordEndpointStatistics_should_save_statistics()
        {
            var repository = new Mock<IEndpointStatsRepository>();
            var monitorSettings = new Mock<IMonitorSettings>();
            var endpointMetricsCoordinator = new Mock<IEndpointMetricsForwarderCoordinator>();

            var endpointHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Offline);
            var endpointId = Guid.NewGuid();
            var endpoint = new Endpoint(TimeCoordinatorMock.Get().Object, new EndpointIdentity(endpointId, "type", "address"), new EndpointMetadata("name", "group", null));

            using (var manager = new EndpointStatsManager(repository.Object, monitorSettings.Object, TimeCoordinatorMock.Get().Object, endpointMetricsCoordinator.Object))
            {
                manager.RecordEndpointStatistics(endpoint.Identity, endpoint.Metadata, endpointHealth);
            }

            repository.Verify(r => r.InsertEndpointStatistics(endpointId, endpointHealth));
        }

        [Fact]
        public async Task Manager_should_delete_old_stats()
        {
            const int expectedRepeats = 3;

            var age = TimeSpan.FromHours(1);
            var utcNow = DateTime.UtcNow;
            var asyncCountdown = new AsyncCountdown("delete old stats", expectedRepeats);
            var asyncCounter = new AsyncCounter();

            var monitorSettings = new Mock<IMonitorSettings>();
            monitorSettings.Setup(s => s.StatsHistoryMaxAge).Returns(age);

            var timeCoordinator = new Mock<ITimeCoordinator>();
            timeCoordinator.Setup(c => c.UtcNow).Returns(utcNow);

            var endpointMetricsCoordinator = new Mock<IEndpointMetricsForwarderCoordinator>();

            var repository = new Mock<IEndpointStatsRepository>();

            repository.Setup(r => r.DeleteStatisticsOlderThan(utcNow - age, It.IsAny<int>())).Returns(() =>
            {
                asyncCountdown.Decrement();
                asyncCounter.Increment();

                const int deletedItemsCount = 127;
                return asyncCounter.Value >= expectedRepeats ? 0 : deletedItemsCount;
            });

            using (new EndpointStatsManager(repository.Object, monitorSettings.Object, timeCoordinator.Object, endpointMetricsCoordinator.Object))
                await asyncCountdown.WaitAsync(MaxTestTime);

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            //it should stop calling cleanup after there is no more items to be cleaned
            Assert.Equal(expectedRepeats, asyncCounter.Value);
        }
    }
}