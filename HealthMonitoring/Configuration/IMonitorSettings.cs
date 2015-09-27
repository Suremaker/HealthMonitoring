using System;

namespace HealthMonitoring.Configuration
{
    public interface IMonitorSettings
    {
        TimeSpan HealthCheckInterval { get; }
        TimeSpan HealthyResponseTimeLimit { get; }
        TimeSpan ShortTimeOut { get; }
        TimeSpan FailureTimeOut { get; }
    }
}