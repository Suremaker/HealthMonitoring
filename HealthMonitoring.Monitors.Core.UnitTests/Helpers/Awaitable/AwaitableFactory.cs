using System;
using System.Collections.Concurrent;
using HealthMonitoring.Monitors.Core.Helpers.Time;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers.Awaitable
{
    public class AwaitableFactory
    {
        private readonly TimeLineProvider _timelineProvider = new TimeLineProvider();
        private readonly ConcurrentQueue<IAsyncTimedEvent> _timeline = new ConcurrentQueue<IAsyncTimedEvent>();

        public AwaitableBuilder<object> Return()
        {
            return new AwaitableBuilder<object>(_timelineProvider, _timeline, () => null);
        }

        public AwaitableBuilder<T> Return<T>(T result)
        {
            return new AwaitableBuilder<T>(_timelineProvider, _timeline, () => result);
        }

        public IAsyncTimedEvent[] GetTimeline()
        {
            return _timeline.ToArray();
        }

        public IStopwatch CreateStopWatch(string tag, TimeSpan elapsed)
        {
            return new MockStopwatch(tag, elapsed, _timelineProvider, _timeline);
        }

        public AwaitableBuilder<object> Throw(Exception exception) { return Throw<object>(exception);}

        public AwaitableBuilder<T> Throw<T>(Exception exception)
        {
            return new AwaitableBuilder<T>(_timelineProvider, _timeline, () => { throw exception; });
        }
    }
}