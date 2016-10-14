using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HealthMonitoring.Integration.PushClient.UnitTests
{
    public class AbstractHealthCheckerTests
    {
        class TestableHealthChecker : AbstractHealthChecker
        {
            private readonly HealthStatus _status;
            private readonly string _value;
            private readonly string _key;

            public TestableHealthChecker(HealthStatus status, string key, string value)
            {
                _status = status;
                _value = value;
                _key = key;
            }

            protected override Task<HealthStatus> OnHealthCheckAsync(Dictionary<string, string> details, CancellationToken cancellationToken)
            {
                details[_key] = _value;
                return Task.FromResult(_status);
            }
        }

        [Fact]
        public async Task HealthChecker_should_capture_default_details()
        {
            var result = await new TestableHealthChecker(HealthStatus.Healthy, "Key", "value").CheckHealthAsync(CancellationToken.None);
            IReadOnlyDictionary<string, string> expected = new Dictionary<string, string>
            {
                {"Key", "value"},
                {"Version",GetType().Assembly.GetName().Version.ToString(4) },
                {"Location", Assembly.GetEntryAssembly()?.Location},
                {"Host",Environment.MachineName }
            };
            Assert.Equal(expected, result.Details);
        }

        [Theory]
        [InlineData(HealthStatus.Offline)]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Unhealthy)]
        [InlineData(HealthStatus.Faulty)]
        public async Task HealthChecker_should_capture_status(HealthStatus expected)
        {
            var result = await new TestableHealthChecker(expected, "key", "value").CheckHealthAsync(CancellationToken.None);
            Assert.Equal(expected, result.Status);
        }
    }
}