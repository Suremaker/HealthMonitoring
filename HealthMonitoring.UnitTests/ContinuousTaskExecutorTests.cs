using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TestUtils;
using HealthMonitoring.TestUtils.Awaitable;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests
{
    public class ContinuousTaskExecutorTests
    {
        private readonly AwaitableFactory _awaitableFactory = new AwaitableFactory();
        private readonly TimeSpan _testTimeout = TimeSpan.FromSeconds(5);

        [Fact]
        public async Task Executor_should_execute_tasks_until_disposal()
        {
            var task1Name = "task1";
            var task2Name = "task2";
            var countdown1 = new AsyncCountdown(task1Name, 10);
            var counter1 = new AsyncCounter();
            var countdown2 = new AsyncCountdown(task2Name, 10);
            var counter2 = new AsyncCounter();

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                Assert.True(executor.TryRegisterTaskFor(task1Name, (item, token) => StartTaskAsync(countdown1, counter1, token)));
                await countdown1.WaitAsync(_testTimeout);

                Assert.True(executor.TryRegisterTaskFor(task2Name, (item, token) => StartTaskAsync(countdown2, counter2, token)));
                await countdown2.WaitAsync(_testTimeout);

                // check that task 1 still works
                await countdown1.ResetTo(10).WaitAsync(_testTimeout);
            }

            var expected1 = counter1.Value;
            var expected2 = counter2.Value;
            await Task.Delay(200);

            Assert.Equal(expected1, counter1.Value);
            Assert.Equal(expected2, counter2.Value);
        }

        [Fact]
        public void Executor_should_not_add_second_task_for_the_same_instance_and_should_not_start_a_second_thread()
        {
            var taskName = "abc";
            var secondTaskStarted = false;
            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                Assert.True(executor.TryRegisterTaskFor(taskName, (item, token) => StartTaskAsync(token)));
                Assert.False(executor.TryRegisterTaskFor(taskName, (item, token) =>
                {
                    secondTaskStarted = true;
                    return StartTaskAsync(token);
                }), "It should not accept second registration");
            }

            Assert.False(secondTaskStarted);
        }

        [Fact]
        public async Task Executor_should_stop_monitoring_item_if_its_task_has_finished_so_it_should_be_possible_to_register_it_again()
        {
            var taskName = "abc";

            var taskFinished = new SemaphoreSlim(0);

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                executor.FinishedTaskFor += item =>
                {
                    if (item == taskName) taskFinished.Release();
                };

                Assert.True(executor.TryRegisterTaskFor(taskName, (item, token) => Task.Delay(25, token)));

                await taskFinished.WaitAsync(_testTimeout);

                Assert.True(executor.TryRegisterTaskFor(taskName, (item, token) => Task.FromResult(0)), "It should be possible to register task again");
            }
        }

        [Fact]
        public async Task Executor_should_continue_processing_other_tasks_when_one_finish()
        {
            var task1Name = "task1";
            var task2Name = "task2";
            var countdown1 = new AsyncCountdown(task1Name, 10);
            var task2Finished = new SemaphoreSlim(0);

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                executor.FinishedTaskFor += item =>
                {
                    if (item == task2Name) task2Finished.Release();
                };

                Assert.True(executor.TryRegisterTaskFor(task1Name, (item, token) => StartTaskAsync(token, b => b.WithCountdown(countdown1))));
                await countdown1.WaitAsync(_testTimeout);

                Assert.True(executor.TryRegisterTaskFor(task2Name, (item, token) => Task.Delay(25, token)));

                await task2Finished.WaitAsync(_testTimeout);

                // check that task 1 still works
                await countdown1.ResetTo(10).WaitAsync(_testTimeout);
            }
        }

        [Fact]
        public async Task Executor_should_continue_processing_other_tasks_if_one_throws_exception()
        {
            var task1Name = "task1";
            var task2Name = "task2";
            var countdown1 = new AsyncCountdown(task1Name, 10);
            var task2Finished = new SemaphoreSlim(0);

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                executor.FinishedTaskFor += item =>
                {
                    if (item == task2Name) task2Finished.Release();
                };

                Assert.True(executor.TryRegisterTaskFor(task1Name, (item, token) => StartTaskAsync(token, b => b.WithCountdown(countdown1))));
                await countdown1.WaitAsync(_testTimeout);

                Assert.True(executor.TryRegisterTaskFor(task2Name, (item, token) => { throw new InvalidOperationException(); }));
                await task2Finished.WaitAsync(_testTimeout);

                // check that task 1 still works
                await countdown1.ResetTo(10).WaitAsync(_testTimeout);
            }
        }

        [Fact]
        public async Task Executor_should_cancel_all_tasks_on_disposal_and_report_all_finished()
        {
            var task1NotCancelled = false;
            var task2NotCancelled = false;
            var task1 = "task1";
            var task2 = "task2";
            var task1Ran = new AsyncCountdown(task1, 1);
            var task2Ran = new AsyncCountdown(task2, 1);
            var completed = new ConcurrentQueue<string>();

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(Mock.Of<ITimeCoordinator>()))
            {
                executor.FinishedTaskFor += item => completed.Enqueue(item);

                executor.TryRegisterTaskFor(task1, async (item, token) =>
                {
                    task1Ran.Decrement();
                    await Task.Delay(_testTimeout, token);
                    task1NotCancelled = true;
                });
                executor.TryRegisterTaskFor(task2, async (item, token) =>
                {
                    task2Ran.Decrement();
                    await Task.Delay(_testTimeout, token);
                    task2NotCancelled = true;
                });

                await task1Ran.WaitAsync(_testTimeout);
                await task2Ran.WaitAsync(_testTimeout);
            }
            Assert.False(task1NotCancelled, "task1NotCancelled");
            Assert.False(task2NotCancelled, "task2NotCancelled");

            CollectionAssert.AreEquivalent(new[] { task1, task2 }, completed);
        }

        [Fact]
        public async Task Executor_should_immediately_break_the_loop_on_cancelation()
        {
            var timeCoordinator = new Mock<ITimeCoordinator>();
            var countdown = new AsyncCountdown("task", 1);
            var taskNotCancelled = false;

            using (var executor = ContinuousTaskExecutor<string>.StartExecutor(timeCoordinator.Object))
            {
                executor.TryRegisterTaskFor("task", async (item, token) =>
                {
                    countdown.Decrement();
                    await Task.Delay(_testTimeout, token);
                    taskNotCancelled = true;
                });

                await countdown.WaitAsync(_testTimeout);
            }

            Assert.False(taskNotCancelled, "Task was not cancelled");
            timeCoordinator.Verify(c => c.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never, "Executor should not trigger any delay");
        }


        private Task StartTaskAsync(CancellationToken token)
        {
            return StartTaskAsync(token, c => c);
        }

        private Task StartTaskAsync(AsyncCountdown countdown, AsyncCounter counter, CancellationToken token)
        {
            return StartTaskAsync(token, c => c.WithCountdown(countdown).WithCounter(counter));
        }

        private async Task StartTaskAsync(CancellationToken token, Func<AwaitableBuilder<object>, AwaitableBuilder<object>> configure)
        {
            while (!token.IsCancellationRequested)
            {
                await configure.Invoke(_awaitableFactory.Return().WithDelay(TimeSpan.FromMilliseconds(10), token))
                    .RunAsync();
            }
        }
    }
}
