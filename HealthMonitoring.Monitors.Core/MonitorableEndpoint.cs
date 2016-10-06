using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Samplers;

namespace HealthMonitoring.Monitors.Core
{
    public class MonitorableEndpoint : IDisposable
    {
        public MonitorableEndpoint(EndpointIdentity identity, IHealthMonitor monitor)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));
            if (monitor == null)
                throw new ArgumentNullException(nameof(monitor));

            Identity = identity;
            Monitor = monitor;
        }

        public EndpointIdentity Identity { get; }
        public IHealthMonitor Monitor { get; }
        public EndpointHealth Health { get; private set; }
        public bool IsDisposed { get; private set; }

        public async Task CheckHealth(IHealthSampler sampler, CancellationToken cancellationToken)
        {
            Health = await sampler.CheckHealthAsync(this, cancellationToken);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public override string ToString()
        {
            return Identity.ToString();
        }
    }
}