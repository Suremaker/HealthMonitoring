using System;

namespace HealthMonitoring.TimeManagement
{
    public interface IStopwatch
    {
        IStopwatch Start();
        IStopwatch Stop();
        TimeSpan Elapsed { get; }
    }
}