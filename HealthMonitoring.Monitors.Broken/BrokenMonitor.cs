using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Broken
{
    public class BrokenMonitor : IHealthMonitor
    {
        public string Name { get { return "broken"; } }
        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public BrokenMonitor()
        {
            throw new Exception("something is broken");
        }

    }
}
