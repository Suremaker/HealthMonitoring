using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    internal class DefaultTimeCoordinator : ITimeCoordinator
    {
        public Task Delay(TimeSpan delay, CancellationToken token)
        {
            return Task.Delay(delay, token);
        }
    }
}