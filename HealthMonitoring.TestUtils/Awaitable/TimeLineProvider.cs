using System;
using System.Threading;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public class TimeLineProvider
    {
        private long _counter;
        private TimeSpan _delta = TimeSpan.FromMilliseconds(1);


        public TimeSpan Capture()
        {
            return TimeSpan.FromTicks(_delta.Ticks * Interlocked.Increment(ref _counter));
        }
    }
}