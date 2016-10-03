using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace HealthMonitoring.TaskManagement
{
    public class ContinuousTaskExecutor<T> : IContinuousTaskExecutor<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger<ContinuousTaskExecutor<T>>();
        private readonly ConcurrentDictionary<T, LazilyCreatedTask> _tasks = new ConcurrentDictionary<T, LazilyCreatedTask>();
        private readonly ManualResetEventSlim _onNewTask = new ManualResetEventSlim();
        private Task _onNewItem;
        private readonly Thread _executionThread;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public event Action<T> FinishedTaskFor;
        public static ContinuousTaskExecutor<T> StartExecutor() { return new ContinuousTaskExecutor<T>(); }

        public bool TryRegisterTaskFor(T item, Func<T, CancellationToken, Task> continuousTaskFactory)
        {
            if (!_tasks.TryAdd(item, new LazilyCreatedTask(() => CreateTaskFor(item, continuousTaskFactory))))
                return false;

            _onNewTask.Set();
            return true;
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _executionThread.Join();
            _cancellation.Dispose();
        }

        private ContinuousTaskExecutor()
        {
            _executionThread = new Thread(Execute) { Name = typeof(T).Name + " TaskExecutor" };
            _executionThread.Start();
        }

        private void Execute()
        {
            int errorCounter = 0;
            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    ProcessTasks();
                    errorCounter = 0;
                }
                catch (OperationCanceledException)
                {
                }
                catch (AggregateException e) when (e.Flatten().InnerExceptions.All(ex => ex is OperationCanceledException))
                {
                }
                catch (Exception e)
                {
                    ++errorCounter;
                    Logger.Fatal("Unexpected exception occurred.", e);
                    Thread.Sleep(TimeSpan.FromSeconds(errorCounter));
                }
            }
            WaitForAllTasksToFinish();
        }

        private void WaitForAllTasksToFinish()
        {
            try
            {
                Task.WaitAll(_tasks.Values.Select(l => l.Task).Cast<Task>().ToArray());
            }
            catch (AggregateException) { }
        }

        private void ProcessTasks()
        {
            Task.WhenAny(GetTasks()).Wait(_cancellation.Token);
        }

        private IEnumerable<Task> GetTasks()
        {
            return _tasks.Values.Select(v => v.Task).Concat(Enumerable.Repeat(WaitForNewItemAsync(), 1));
        }

        private async Task CreateTaskFor(T item, Func<T, CancellationToken, Task> continousTaskFactory)
        {
            try
            {
                await continousTaskFactory.Invoke(item, _cancellation.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (AggregateException e) when (e.Flatten().InnerException is OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected task failure", e);
            }

            FinalizeTask(item);
        }

        private void FinalizeTask(T item)
        {
            LazilyCreatedTask value;
            if (!_tasks.TryRemove(item, out value))
                return;

            Logger.Info($"Finished processing {typeof(T)}: {item}");
            FinishedTaskFor?.Invoke(item);
        }

        private Task WaitForNewItemAsync()
        {
            if (_onNewItem != null && !_onNewItem.IsCompleted)
                return _onNewItem;

            return _onNewItem = Task.Run(() => WaitForNewEndpoint());
        }

        private void WaitForNewEndpoint()
        {
            _onNewTask.Wait(_cancellation.Token);
            _onNewTask.Reset();
        }

        /// <summary>
        /// It would start task only when requested for a first time. If never requested, task will not be created.
        /// </summary>
        private class LazilyCreatedTask
        {
            private Task _task;
            private readonly Func<Task> _taskFactory;

            public LazilyCreatedTask(Func<Task> taskFactory)
            {
                _taskFactory = taskFactory;
            }

            public Task Task
            {
                get
                {
                    if (_task == null)
                        InitializeTask();
                    return _task;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void InitializeTask()
            {
                if (_task != null)
                    return;
                _task = _taskFactory.Invoke();
            }
        }
    }
}
