using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    internal class CachedValue<T>
    {
        private readonly TimeSpan _cacheDuration;
        private readonly Func<Task<T>> _refreshMethod;
        private T _cachedValue;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private TimeSpan? _lastUpdateTime;

        public CachedValue(TimeSpan cacheDuration, Func<Task<T>> refreshMethod)
        {
            _cacheDuration = cacheDuration;
            _refreshMethod = refreshMethod;
        }

        public async Task<T> GetValueAsync()
        {
            if (_lastUpdateTime == null || _lastUpdateTime + _cacheDuration < _stopwatch.Elapsed)
            {
                _cachedValue = await _refreshMethod.Invoke();
                _lastUpdateTime = _stopwatch.Elapsed;
            }
            return _cachedValue;
        }
    }
}