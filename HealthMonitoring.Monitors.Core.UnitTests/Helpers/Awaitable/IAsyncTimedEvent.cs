using System;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers.Awaitable
{
    public interface IAsyncTimedEvent
    {
        string Tag { get; }
        TimeSpan Finished { get; }
        TimeSpan Started { get; }
    }
}