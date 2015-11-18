using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public class ThrottlingSampler : IHealthSampler
    {
        private readonly IHealthSampler _sampler;
        private readonly IDictionary<string, SemaphoreSlim> _throttles;

        public ThrottlingSampler(IHealthSampler sampler, IThrottlingSettings settings)
        {
            _sampler = sampler;
            _throttles = settings.Throttling.ToDictionary(kv => kv.Key, kv => new SemaphoreSlim(kv.Value));
        }

        public async Task<EndpointHealth> CheckHealth(Endpoint endpoint, CancellationToken cancellationToken)
        {
            using (await Begin(endpoint.MonitorType, cancellationToken))
                return await _sampler.CheckHealth(endpoint, cancellationToken);
        }

        private async Task<IDisposable> Begin(string monitorType, CancellationToken cancellationToken)
        {
            SemaphoreSlim semaphore;
            if (!_throttles.TryGetValue(monitorType, out semaphore))
                return new NotThrottledContext();
            await semaphore.WaitAsync(cancellationToken);
            return new ThrottledContext(semaphore);
        }

        private class ThrottledContext : IDisposable
        {
            private SemaphoreSlim _semaphore;

            public ThrottledContext(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_semaphore != null)
                    _semaphore.Release();
                _semaphore = null;
            }
        }
        private class NotThrottledContext : IDisposable
        {
            public void Dispose() { }
        }
    }
}
