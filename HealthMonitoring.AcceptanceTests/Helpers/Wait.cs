using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public static class Wait
    {
        public static T Until<T>(TimeSpan timeout, Func<T> selector, Func<T, bool> predicate, string errorMessage)
        {
            var valuesCollated = new List<T>();
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < timeout)
            {
                var value = selector();

                if (predicate(value))
                    return value;

                if (!valuesCollated.Contains(value))
                    valuesCollated.Add(value);

                Thread.Sleep(100);
            }

            string suffixtMessage = GetSuffixMessage(valuesCollated);

            throw new TimeoutException($"{errorMessage} " + suffixtMessage);
        }

        private static string GetSuffixMessage<T>(List<T> valuesCollated)
        {
            string suffixtMessage = valuesCollated.Aggregate("Values collated: [", (current, t) => current + $", {t}");

            suffixtMessage = $"{suffixtMessage.Replace(" [,", " [")}]";

            return suffixtMessage;
        }
    }
}