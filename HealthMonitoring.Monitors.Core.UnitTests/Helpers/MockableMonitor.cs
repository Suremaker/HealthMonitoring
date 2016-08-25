using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers
{
    class MockableMonitor : IHealthMonitor
    {
        private Func<string, CancellationToken, Task<HealthInfo>> _healthFunction = (a, c) => { throw new NotImplementedException(); };
        public string Name => "mockable";
        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            return _healthFunction?.Invoke(address, cancellationToken);
        }

        public void ExpectFor(string address, Func<CancellationToken, Task<HealthInfo>> action)
        {
            _healthFunction = (a, c) =>
            {
                Assert.Equal(address, a);
                return action(c);
            };
        }
    }
}