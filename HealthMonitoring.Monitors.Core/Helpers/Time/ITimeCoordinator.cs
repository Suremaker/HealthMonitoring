using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Core.Helpers.Time
{
    public interface ITimeCoordinator
    {
        Task Delay(TimeSpan interval, CancellationToken cancellationToken);
        IStopwatch CreateStopWatch();
        DateTime UtcNow { get; }
    }
}