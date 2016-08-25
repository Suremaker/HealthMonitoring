using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Monitors.Core.Samplers;

namespace HealthMonitoring.Monitors.Core
{
    public class EndpointMonitor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger<EndpointMonitor>();
        private readonly IMonitorSettings _settings;
        private readonly CancellationTokenSource _cancellation;
        private readonly Thread _monitor;
        private readonly ConcurrentDictionary<MonitorableEndpoint, Task<MonitorableEndpoint>> _tasks = new ConcurrentDictionary<MonitorableEndpoint, Task<MonitorableEndpoint>>();
        private readonly ManualResetEventSlim _onNewTask = new ManualResetEventSlim();
        private Task<MonitorableEndpoint> _onNewEndpoint;
        private readonly MonitorableEndpointRegistry _monitorableEndpointRegistry;
        private readonly IHealthSampler _sampler;
        private readonly Random _randomizer = new Random();

        public EndpointMonitor(MonitorableEndpointRegistry monitorableEndpointRegistry, IHealthSampler sampler, IMonitorSettings settings)
        {
            _monitorableEndpointRegistry = monitorableEndpointRegistry;
            _sampler = sampler;
            _settings = settings;
            _monitorableEndpointRegistry.NewEndpointAdded += HandleNewEndpoint;
            _cancellation = new CancellationTokenSource();

            foreach (var endpoint in _monitorableEndpointRegistry.Endpoints)
                _tasks.AddOrUpdate(endpoint, CreateTaskFor, (e, currentTask) => currentTask);

            _monitor = new Thread(Start) { Name = "Monitor" };
            _monitor.Start();
        }

        private void Start()
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
                catch (AggregateException e)
                {
                    if (e.Flatten().InnerExceptions.FirstOrDefault() is OperationCanceledException)
                        continue;
                    ++errorCounter;
                    Logger.ErrorFormat("Monitoring error ({0} occurrence): {1}", errorCounter, e.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(errorCounter));
                }
                catch (Exception e)
                {
                    ++errorCounter;
                    Logger.ErrorFormat("Monitoring error ({0} occurrence): {1}", errorCounter, e.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(errorCounter));
                }
            }
        }

        private void ProcessTasks()
        {
            var task = Task.WhenAny(GetTasks());
            task.Wait(_cancellation.Token);
            Task<MonitorableEndpoint> endpoint;
            if (task.Result.Result != null)
                _tasks.TryRemove(task.Result.Result, out endpoint);
        }

        private IEnumerable<Task<MonitorableEndpoint>> GetTasks()
        {
            return _tasks.Values.Concat(Enumerable.Repeat(GetWaitForNewEndpointTask(), 1));
        }

        public void Dispose()
        {
            _monitorableEndpointRegistry.NewEndpointAdded -= HandleNewEndpoint;
            _cancellation.Cancel();
            _monitor.Join();
        }

        private async Task<MonitorableEndpoint> CreateTaskFor(MonitorableEndpoint endpoint)
        {
            await Task.Delay(GetRandomizedDelay());
            while (!_cancellation.IsCancellationRequested && !endpoint.IsDisposed)
            {
                var delay = Task.Delay(_settings.HealthCheckInterval);
                await endpoint.CheckHealth(_sampler, _cancellation.Token);
                await delay;
            }
            return endpoint;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TimeSpan GetRandomizedDelay()
        {
            var delay = _randomizer.NextDouble() * _settings.HealthCheckInterval.TotalMilliseconds;
            return TimeSpan.FromMilliseconds(delay);
        }

        private void HandleNewEndpoint(MonitorableEndpoint endpoint)
        {
            if (_tasks.TryAdd(endpoint, CreateTaskFor(endpoint)))
                _onNewTask.Set();
        }

        private Task<MonitorableEndpoint> GetWaitForNewEndpointTask()
        {
            if (_onNewEndpoint != null && !_onNewEndpoint.IsCompleted)
                return _onNewEndpoint;

            return _onNewEndpoint = Task.Run(() => WaitForNewEndpoint());
        }

        private MonitorableEndpoint WaitForNewEndpoint()
        {
            _onNewTask.Wait(_cancellation.Token);
            _onNewTask.Reset();
            return null;
        }
    }
}
