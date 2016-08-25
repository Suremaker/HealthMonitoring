using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HealthMonitoring.Monitors.Core.Helpers
{
    public class OutgoingQueue<T>
    {
        private readonly BlockingCollection<T> _collection = new BlockingCollection<T>();
        private readonly int _maxCapacity;

        public OutgoingQueue(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
        }

        public void Enqueue(T item)
        {
            _collection.Add(item);
            T garbage;
            while (_collection.Count > _maxCapacity)
                _collection.TryTake(out garbage);
        }

        public T[] Dequeue(int maxCount, TimeSpan maxWaitTime, CancellationToken cancellation)
        {
            var retrieved = new List<T>(maxCount);
            var watch = Stopwatch.StartNew();
            while (retrieved.Count < maxCount)
            {
                var remainingTime = maxWaitTime - watch.Elapsed;

                if (remainingTime.Ticks < 0)
                    break;

                T item;
                if (!_collection.TryTake(out item, (int)remainingTime.TotalMilliseconds, cancellation))
                    break;

                retrieved.Add(item);
            }
            return retrieved.ToArray();
        }
    }
}