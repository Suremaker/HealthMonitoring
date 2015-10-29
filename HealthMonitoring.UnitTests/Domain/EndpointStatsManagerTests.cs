using System;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class EndpointStatsManagerTests
    {
        [Fact]
        public void RecordEndpointStatistics_should_save_statistics()
        {
            var repository = new Mock<IEndpointStatsRepository>();
            var monitorSettings = new Mock<IMonitorSettings>();

            var endpointHealth = EndpointHealth.FromResult(DateTime.UtcNow, new HealthInfo(HealthStatus.Offline, TimeSpan.Zero), TimeSpan.Zero);
            var endpointId = Guid.NewGuid();

            using (var manager = new EndpointStatsManager(repository.Object, monitorSettings.Object))
                manager.RecordEndpointStatistics(endpointId, endpointHealth);
            repository.Verify(r => r.InsertEndpointStatistics(endpointId, endpointHealth));
        }

        [Fact]
        public void Manager_should_delete_old_stats()
        {
            var repository = new Mock<IEndpointStatsRepository>();
            var monitorSettings = new Mock<IMonitorSettings>();
            var age = TimeSpan.FromHours(1);
            var delta = TimeSpan.FromSeconds(5);
            monitorSettings.Setup(s => s.StatsHistoryMaxAge).Returns(age);
            using (new EndpointStatsManager(repository.Object, monitorSettings.Object))
            {
                repository.Verify(r => r.DeleteStatisticsOlderThan(It.Is<DateTime>(t => DateTime.UtcNow - t > age.Subtract(delta) && DateTime.UtcNow - t < age.Add(delta))));
            }
        }
    }
}