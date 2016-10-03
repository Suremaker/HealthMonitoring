using System;
using System.Collections.Concurrent;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public class MockStopwatch : IStopwatch, IAsyncTimedEvent
    {
        private readonly TimeLineProvider _timelineProvider;
        private readonly ConcurrentQueue<IAsyncTimedEvent> _timeline;

        public MockStopwatch(string tag, TimeSpan elapsed, TimeLineProvider timelineProvider, ConcurrentQueue<IAsyncTimedEvent> timeline)
        {
            Tag = tag;
            _timelineProvider = timelineProvider;
            _timeline = timeline;
            Elapsed = elapsed;
        }

        public IStopwatch Start()
        {
            Started = _timelineProvider.Capture();
            _timeline.Enqueue(this);
            return this;
        }

        public IStopwatch Stop()
        {
            Finished = _timelineProvider.Capture();
            return this;
        }

        public TimeSpan Elapsed { get; }
        public string Tag { get; }
        public TimeSpan Finished { get; private set; }
        public TimeSpan Started { get; private set; }
    }
}