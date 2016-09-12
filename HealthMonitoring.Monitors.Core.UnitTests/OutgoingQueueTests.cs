using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using HealthMonitoring.Monitors.Core.Helpers;
using Xunit;

namespace HealthMonitoring.Monitors.Core.UnitTests
{
    public class OutgoingQueueTests
    {
        private static readonly TimeSpan AcceptableTimeDelta = TimeSpan.FromMilliseconds(300);

        [Fact]
        public void Queue_should_override_old_items_if_limit_is_reached()
        {
            var maxCapacity = 10;
            var queue = new OutgoingQueue<int>(maxCapacity);
            var all = Enumerable.Range(0, maxCapacity * 2).ToArray();
            foreach (var i in all)
                queue.Enqueue(i);

            var actual = queue.Dequeue(queue.Count, TimeSpan.Zero, CancellationToken.None);
            var expected = all.Skip(all.Length - maxCapacity).ToArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Queue_should_dequeue_item_once()
        {
            var queue = new OutgoingQueue<int>(10);
            queue.Enqueue(1);
            queue.Enqueue(2);
            Assert.Equal(new[] { 1 }, queue.Dequeue(1, TimeSpan.Zero, CancellationToken.None));

            Assert.Equal(new[] { 2 }, queue.Dequeue(1, TimeSpan.Zero, CancellationToken.None));
        }

        [Fact]
        public void Queue_should_return_immediately_a_bucket_if_there_is_enough_elements_to_fill_it()
        {
            var maxCapacity = 1000;
            var bucketSize = 500;
            var queue = new OutgoingQueue<int>(maxCapacity);
            for (var i = 0; i < maxCapacity; i++)
                queue.Enqueue(i);

            var watch = Stopwatch.StartNew();
            var items = queue.Dequeue(bucketSize, TimeSpan.FromSeconds(2), CancellationToken.None);
            watch.Stop();

            Assert.Equal(bucketSize, items.Length);
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1), "watch.Elapsed < TimeSpan.FromSeconds(1)");
        }

        [Fact]
        public void Queue_should_return_available_items_if_timeout_reached()
        {
            var bucketSize = 500;
            var queue = new OutgoingQueue<int>(bucketSize + 1);
            var availableItemsCount = bucketSize - 1;
            for (var i = 0; i < availableItemsCount; i++)
                queue.Enqueue(i);

            var timeout = TimeSpan.FromMilliseconds(500);
            var watch = Stopwatch.StartNew();
            var items = queue.Dequeue(bucketSize, timeout, CancellationToken.None);
            watch.Stop();

            Assert.Equal(availableItemsCount, items.Length);
            Assert.True((watch.Elapsed - timeout).Duration() < AcceptableTimeDelta, "Expected full timeout");
        }

        [Fact]
        public void Queue_should_return_available_items_if_cancelled()
        {
            var bucketSize = 500;
            var queue = new OutgoingQueue<int>(bucketSize + 1);
            var availableItemsCount = bucketSize - 1;
            for (var i = 0; i < availableItemsCount; i++)
                queue.Enqueue(i);

            var timeout = TimeSpan.FromMilliseconds(500);
            var watch = Stopwatch.StartNew();
            var maxWaitTime = TimeSpan.FromSeconds(5);
            var items = queue.Dequeue(bucketSize, maxWaitTime, new CancellationTokenSource(timeout).Token);
            watch.Stop();

            Assert.Equal(availableItemsCount, items.Length);
            Assert.True((watch.Elapsed - timeout).Duration() < maxWaitTime, $"Expected task cancellation before {maxWaitTime}");
        }

        [Fact]
        public void Queue_should_return_empty_array_if_there_is_no_elements()
        {
            Assert.Equal(new int[0], new OutgoingQueue<int>(100).Dequeue(1, TimeSpan.Zero, CancellationToken.None));
        }
    }
}