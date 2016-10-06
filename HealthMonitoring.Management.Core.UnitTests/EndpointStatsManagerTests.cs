using System;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
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

            var endpointHealth = new EndpointHealth(DateTime.UtcNow, TimeSpan.Zero, EndpointStatus.Offline);
            var endpointId = Guid.NewGuid();

            using (var manager = new EndpointStatsManager(repository.Object, monitorSettings.Object, Mock.Of<ITimeCoordinator>()))
                manager.RecordEndpointStatistics(endpointId, endpointHealth);
            repository.Verify(r => r.InsertEndpointStatistics(endpointId, endpointHealth));
        }

        [Fact]
        public async Task Manager_should_delete_old_stats()
        {
            var age = TimeSpan.FromHours(1);
            var utcNow = DateTime.UtcNow;
            var asyncCountdown = new AsyncCountdown("delete old stats", 1);

            var monitorSettings = new Mock<IMonitorSettings>();
            monitorSettings.Setup(s => s.StatsHistoryMaxAge).Returns(age);

            var timeCoordinator = new Mock<ITimeCoordinator>();
            timeCoordinator.Setup(c => c.UtcNow).Returns(utcNow);

            var repository = new Mock<IEndpointStatsRepository>();
            repository.Setup(r => r.DeleteStatisticsOlderThan(utcNow - age)).Callback(() => asyncCountdown.Decrement());

            using (new EndpointStatsManager(repository.Object, monitorSettings.Object, timeCoordinator.Object))
                await asyncCountdown.WaitAsync(MaxTestTime);
        }
    }
}