using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public interface ITimeCoordinator
    {
        Task Delay(TimeSpan delay, CancellationToken token);
    }
}