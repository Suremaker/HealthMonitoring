using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Samplers;
using HealthMonitoring.Monitors.Core.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class ThrottlingSamplerTests
    {
        [Fact]
        public async Task Sampler_should_throttle_calls()
        {
            var monitorType = "test";
            var throttling = 2;
            var total = 3;

            var settings = SetupThrottling(new Dictionary<string, int> { { monitorType, throttling } });
            var endpoint = SetupEndpoint(monitorType);
            var manualReset = new ManualResetEventSlim();
            var sampler = SetupSampler(endpoint, manualReset);

            var throttlingSampler = new ThrottlingSampler(sampler.Object, settings.Object);
            var token = new CancellationToken();

            var tasks = Enumerable.Range(0, total)
                .Select(n => throttlingSampler.CheckHealthAsync(endpoint, token))
                .ToArray();
            var allFinishedTask = Task.WhenAll(tasks);

            await Task.Delay(250);
            Assert.False(allFinishedTask.IsCompleted, "No tasks should finish yet");
            sampler.Verify(s => s.CheckHealthAsync(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(throttling));

            manualReset.Set();
            await allFinishedTask;
            sampler.Verify(s => s.CheckHealthAsync(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(total));
        }

        [Fact]
        public async Task Sampler_should_not_throttle_calls_if_throttling_is_not_specified()
        {
            var monitorType = "test";
            var total = 3;

            var settings = SetupThrottling(new Dictionary<string, int>());
            var endpoint = SetupEndpoint(monitorType);
            var manualReset = new ManualResetEventSlim();
            var sampler = SetupSampler(endpoint, manualReset);

            var throttlingSampler = new ThrottlingSampler(sampler.Object, settings.Object);
            var token = new CancellationToken();

            var tasks = Enumerable.Range(0, total)
                .Select(n => throttlingSampler.CheckHealthAsync(endpoint, token))
                .ToArray();
            var allFinishedTask = Task.WhenAll(tasks);

            Assert.False(allFinishedTask.Wait(250), "No tasks should finish yet");
            sampler.Verify(s => s.CheckHealthAsync(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(total));

            manualReset.Set();
            await allFinishedTask;
        }

        private static MonitorableEndpoint SetupEndpoint(string monitorType)
        {
            return new MonitorableEndpoint(new EndpointIdentity(Guid.NewGuid(), "blah", "address"), MonitorMock.Mock(monitorType));
        }

        private static Mock<IThrottlingSettings> SetupThrottling(Dictionary<string, int> throttling)
        {
            var settings = new Mock<IThrottlingSettings>();
            settings.Setup(s => s.Throttling).Returns(throttling);
            return settings;
        }

        private static Mock<IHealthSampler> SetupSampler(MonitorableEndpoint endpoint, ManualResetEventSlim manualReset)
        {
            var sampler = new Mock<IHealthSampler>();
            sampler
                .Setup(s => s.CheckHealthAsync(endpoint, It.IsAny<CancellationToken>()))
                .Returns(async (MonitorableEndpoint e, CancellationToken ct) =>
                {
                    await Task.Run(() => manualReset.Wait(ct));
                    return null;
                });
            return sampler;
        }
    }
}