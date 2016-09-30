using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.TimeManagement
{
    public interface ITimeCoordinator
    {
        Task Delay(TimeSpan interval, CancellationToken cancellationToken);
        IStopwatch CreateStopWatch();
        DateTime UtcNow { get; }
    }
}