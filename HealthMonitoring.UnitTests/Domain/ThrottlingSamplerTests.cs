using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class ThrottlingSamplerTests
    {
        [Fact]
        public void Sampler_should_throttle_calls()
        {
            var monitorType = "test";
            var throttling = 2;
            var total = 3;

            var settings = SetupThrottling(new Dictionary<string, int> {{monitorType, throttling}});
            var endpoint = SetupEndpoint(monitorType);
            var manualReset = new ManualResetEventSlim();
            var sampler = SetupSampler(endpoint, manualReset);

            var throttlingSampler = new ThrottlingSampler(sampler.Object, settings.Object);
            var token = new CancellationToken();

            var tasks = Enumerable.Range(0, total)
                .Select(n => throttlingSampler.CheckHealth(endpoint, token))
                .ToArray();
            var allFinishedTask = Task.WhenAll(tasks);

            Assert.False(allFinishedTask.Wait(250), "No tasks should finish yet");
            sampler.Verify(s => s.CheckHealth(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(throttling));

            manualReset.Set();
            allFinishedTask.Wait();
            sampler.Verify(s => s.CheckHealth(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(total));
        }

        [Fact]
        public void Sampler_should_not_throttle_calls_if_throttling_is_not_specified()
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
                .Select(n => throttlingSampler.CheckHealth(endpoint, token))
                .ToArray();
            var allFinishedTask = Task.WhenAll(tasks);

            Assert.False(allFinishedTask.Wait(250), "No tasks should finish yet");
            sampler.Verify(s => s.CheckHealth(endpoint, It.IsAny<CancellationToken>()), Times.Exactly(total));

            manualReset.Set();
            allFinishedTask.Wait();
        }

        private static Endpoint SetupEndpoint(string monitorType)
        {
            return new Endpoint(Guid.NewGuid(), Helpers.MonitorMock.Mock(monitorType), "address", "name", "group", new[] { "t1", "t2" });
        }

        private static Mock<IThrottlingSettings> SetupThrottling(Dictionary<string, int> throttling)
        {
            var settings = new Mock<IThrottlingSettings>();
            settings.Setup(s => s.Throttling).Returns(throttling);
            return settings;
        }

        private static Mock<IHealthSampler> SetupSampler(Endpoint endpoint, ManualResetEventSlim manualReset)
        {
            var sampler = new Mock<IHealthSampler>();
            sampler
                .Setup(s => s.CheckHealth(endpoint, It.IsAny<CancellationToken>()))
                .Returns(async (Endpoint e, CancellationToken ct) =>
                {
                    await Task.Run(() => manualReset.Wait(ct));
                    return null;
                });
            return sampler;
        }
    }
}