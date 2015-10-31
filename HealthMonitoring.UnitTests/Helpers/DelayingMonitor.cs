using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.UnitTests.Helpers
{
    internal class DelayingMonitor : IHealthMonitor
    {
        private readonly TimeSpan _delay;

        public DelayingMonitor(TimeSpan delay)
        {
            _delay = delay;
        }

        public string Name { get { return "delaying"; } }
        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            return new HealthInfo(HealthStatus.Healthy);
        }
    }
}