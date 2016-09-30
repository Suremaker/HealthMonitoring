using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.TimeManagement
{
    public class TimeCoordinator : ITimeCoordinator
    {
        public Task Delay(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        public IStopwatch CreateStopWatch()
        {
            return new DefaultStopwatch();
        }

        public DateTime UtcNow => DateTime.UtcNow;

        private class DefaultStopwatch : IStopwatch
        {
            private readonly Stopwatch _watch = new Stopwatch();
            public IStopwatch Start()
            {
                _watch.Start();
                return this;
            }

            public IStopwatch Stop()
            {
                _watch.Stop();
                return this;
            }

            public TimeSpan Elapsed => _watch.Elapsed;
        }
    }
}