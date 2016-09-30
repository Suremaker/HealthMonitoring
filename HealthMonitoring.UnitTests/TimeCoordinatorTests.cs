using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.TimeManagement;
using Xunit;

namespace HealthMonitoring.UnitTests
{
    public class TimeCoordinatorTests
    {
        private readonly ITimeCoordinator _coordinator = new TimeCoordinator();

        [Fact]
        public async Task CreateStopWatch_should_return_object_measuring_time()
        {
            var watch = _coordinator.CreateStopWatch();

            //before start
            await Task.Delay(200);
            Assert.Equal(TimeSpan.Zero, watch.Elapsed);

            //real measurement
            var expected = Stopwatch.StartNew();
            watch.Start();
            await Task.Delay(500);
            watch.Stop();
            expected.Stop();
            Assert.True((expected.Elapsed - watch.Elapsed).Duration() < TimeSpan.FromMilliseconds(300), "(expected.Elapsed - watch.Elapsed) < TimeSpan.FromMilliseconds(100)");

            //after stop
            var afterStop = watch.Elapsed;
            await Task.Delay(200);
            Assert.Equal(afterStop, watch.Elapsed);
        }

        [Fact]
        public void CreateStopWatch_should_return_new_instances_of_stopwatches()
        {
            Assert.NotSame(_coordinator.CreateStopWatch(), _coordinator.CreateStopWatch());
        }

        [Fact]
        public void UtcNow_should_return_proper_time()
        {
            Assert.True((DateTime.UtcNow - _coordinator.UtcNow).Duration() < TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Delay_should_postpone_task_execution()
        {
            var watch = Stopwatch.StartNew();
            await _coordinator.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
            watch.Stop();
            Assert.True(watch.Elapsed > TimeSpan.FromMilliseconds(400), "watch.Elapsed > TimeSpan.FromMilliseconds(400)");
        }

        [Fact]
        public async Task Delay_should_be_cancellable()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(() => _coordinator.Delay(TimeSpan.FromSeconds(5), new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token));
        }
    }
}