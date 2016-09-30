using System;
using System.Threading.Tasks;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public class AsyncTimedEvent : IAsyncTimedEvent
    {
        private readonly TimeLineProvider _timeline;
        public string Tag { get; }

        public AsyncTimedEvent(string tag, TimeLineProvider timeline)
        {
            Tag = tag;
            _timeline = timeline;
        }

        public async Task<T> Await<T>(Func<Task<T>> inner)
        {
            Started = _timeline.Capture();
            try
            {
                return await inner.Invoke();
            }
            finally
            {
                Finished = _timeline.Capture();
            }
        }

        public TimeSpan Finished { get; private set; }

        public TimeSpan Started { get; private set; }
    }
}