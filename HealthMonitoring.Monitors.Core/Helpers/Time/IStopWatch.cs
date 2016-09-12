using System;

namespace HealthMonitoring.Monitors.Core.Helpers.Time
{
    public interface IStopwatch
    {
        IStopwatch Start();
        IStopwatch Stop();
        TimeSpan Elapsed { get; }
    }
}