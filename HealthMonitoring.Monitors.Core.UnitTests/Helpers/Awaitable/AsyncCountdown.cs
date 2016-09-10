using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Core.UnitTests.Helpers.Awaitable
{
    public class AsyncCountdown
    {
        private readonly string _name;
        private int _number;
        private readonly SemaphoreSlim _semaphore;

        public AsyncCountdown(string name, int number)
        {
            _name = name;
            _number = number;
            _semaphore = new SemaphoreSlim(0);
        }

        public void Decrement()
        {
            if (Interlocked.Decrement(ref _number) == 0)
                _semaphore.Release();
        }

        public async Task WaitAsync(TimeSpan timeout)
        {
            if (!await _semaphore.WaitAsync(timeout))
                throw new TimeoutException(_name);
        }

        public AsyncCountdown ResetTo(int number)
        {
            while (_semaphore.Wait(0)) { }
            _number = number;
            return this;
        }
    }
}