using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public class AwaitableFactory
    {
        private readonly TimeLineProvider _timelineProvider = new TimeLineProvider();
        private readonly ConcurrentQueue<IAsyncTimedEvent> _timeline = new ConcurrentQueue<IAsyncTimedEvent>();

        public AwaitableBuilder<object> Return()
        {
            return Execute<object>(() => null);
        }

        public AwaitableBuilder<T> Return<T>(T result)
        {
            return Execute(() => result);
        }

        public AwaitableBuilder<object> Execute(Action action)
        {
            return Execute<object>(() => { action(); return null; });
        }

        public AwaitableBuilder<T> Execute<T>(Func<T> action)
        {
            return new AwaitableBuilder<T>(_timelineProvider, _timeline, action);
        }

        public IAsyncTimedEvent[] GetTimeline()
        {
            return _timeline.ToArray();
        }

        public IEnumerable<string> GetOrderedTimelineEvents()
        {
            return GetTimeline().SelectMany(t => new [] {Tuple.Create(t.Tag+"_start",t.Started), Tuple.Create(t.Tag + "_finish", t.Finished) }).OrderBy(x => x.Item2).Select(x => x.Item1);
        }

        public IStopwatch CreateStopWatch(string tag, TimeSpan elapsed)
        {
            return new MockStopwatch(tag, elapsed, _timelineProvider, _timeline);
        }

        public AwaitableBuilder<object> Throw(Exception exception) { return Throw<object>(exception); }

        public AwaitableBuilder<T> Throw<T>(Exception exception)
        {
            return new AwaitableBuilder<T>(_timelineProvider, _timeline, () => { throw exception; });
        }
    }
}