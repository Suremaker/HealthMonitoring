using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers
{
    internal class TestableHealthMonitor : IHealthMonitor
    {
        private readonly ConcurrentQueue<Tuple<string, TimeSpan>> _calls = new ConcurrentQueue<Tuple<string, TimeSpan>>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        public string Name => "test";
        public IEnumerable<Tuple<string, TimeSpan>> Calls => _calls;
        public TimeSpan Delay { get; set; }

        public void StartWatch()
        {
            _stopwatch.Start();
        }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            await Task.Delay(Delay, cancellationToken);
            _calls.Enqueue(Tuple.Create(address, _stopwatch.Elapsed));
            return new HealthInfo(HealthStatus.Healthy, new Dictionary<string, string>());
        }
    }
}