using System;
using System.Diagnostics;
using System.Threading;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public static class Wait
    {
        public static T Until<T>(TimeSpan timeout, Func<T> selector, Func<T, bool> predicate, string errorMessage)
        {
            var sw = new Stopwatch();
            sw.Start();
            var value = default(T);
            while (sw.Elapsed < timeout)
            {
                value = selector();
                if (predicate(value))
                    return value;
                Thread.Sleep(100);
            }
            throw new TimeoutException(string.Format("{0}, Last value: {1}", errorMessage, value));
        }
    }
}