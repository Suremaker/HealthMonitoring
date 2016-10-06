using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HealthMonitoring.Queueing
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
            try
            {
                var watch = Stopwatch.StartNew();
                while (retrieved.Count < maxCount)
                {
                    var remainingTime = maxWaitTime - watch.Elapsed;

                    T item;
                    if (!_collection.TryTake(out item, Math.Max(0, (int)remainingTime.TotalMilliseconds), cancellation))
                        break;

                    retrieved.Add(item);
                }
            }
            catch (OperationCanceledException) { }
            return retrieved.ToArray();
        }

        public int Count => _collection.Count;
    }
}