using System;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public interface IAsyncTimedEvent
    {
        string Tag { get; }
        TimeSpan Finished { get; }
        TimeSpan Started { get; }
    }
}