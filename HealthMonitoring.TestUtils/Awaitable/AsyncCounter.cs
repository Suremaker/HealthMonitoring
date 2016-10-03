using System.Threading;

namespace HealthMonitoring.TestUtils.Awaitable
{
    public class AsyncCounter
    {
        private int _counter;

        public int Value => _counter;

        public void Increment()
        {
            Interlocked.Increment(ref _counter);
        }
    }
}