using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers.Awaitable
{
    public class AwaitableBuilder<T>
    {
        private Func<Task<T>> _task;
        private readonly TimeLineProvider _timelineProvider;
        private readonly ConcurrentQueue<IAsyncTimedEvent> _timeline;

        internal AwaitableBuilder(TimeLineProvider timelineProvider, ConcurrentQueue<IAsyncTimedEvent> timeline, Func<T> result)
        {
            _timelineProvider = timelineProvider;
            _timeline = timeline;
            _task = async () =>
            {
                await Task.Yield();
                return result();
            };
        }

        public AwaitableBuilder<T> WithTimeline(string tag)
        {
            var inner = _task;
            _task = () =>
            {
                var awaitable = new AsyncTimedEvent(tag, _timelineProvider);
                _timeline.Enqueue(awaitable);
                return awaitable.Await(inner);
            };
            return this;
        }

        public AwaitableBuilder<T> WithDelay(TimeSpan delay) => WithDelay(delay, CancellationToken.None);
        public AwaitableBuilder<T> WithDelay(TimeSpan delay, CancellationToken cancellationToken)
        {
            var inner = _task;
            _task = async () =>
            {
                await Task.Delay(delay, cancellationToken);
                return await inner();
            };
            return this;
        }

        public AwaitableBuilder<T> WithCountdown(AsyncCountdown countdown)
        {
            var inner = _task;
            _task = async () =>
            {
                try
                {
                    return await inner.Invoke();
                }
                finally
                {
                    countdown.Decrement();
                }
            };
            return this;
        }

        public AwaitableBuilder<T> WithCounter(AsyncCounter counter)
        {
            var inner = _task;
            _task = async () =>
            {
                try
                {
                    return await inner.Invoke();
                }
                finally
                {
                    counter.Increment();
                }
            };
            return this;
        }

        public Task<T> RunAsync()
        {
            return _task.Invoke();
        }
    }
}