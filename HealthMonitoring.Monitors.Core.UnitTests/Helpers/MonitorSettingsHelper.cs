using System;
using HealthMonitoring.Configuration;
using Moq;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers
{
    class MonitorSettingsHelper
    {
        public static IMonitorSettings ConfigureSettings(TimeSpan interval)
        {
            var settings = new Mock<IMonitorSettings>();
            settings.Setup(s => s.HealthCheckInterval).Returns(interval);
            settings.Setup(s => s.FailureTimeOut).Returns(TimeSpan.FromSeconds(50));
            settings.Setup(s => s.HealthyResponseTimeLimit).Returns(TimeSpan.FromSeconds(50));
            settings.Setup(s => s.ShortTimeOut).Returns(TimeSpan.FromSeconds(50));
            return settings.Object;
        }
    }
}