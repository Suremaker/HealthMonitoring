using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange;
using HealthMonitoring.Monitors.Core.Helpers;
using HealthMonitoring.Monitors.Core.Registers;

namespace HealthMonitoring.Monitors.Core
{
    public class MonitorDataExchange : IDisposable, IEndpointHealthUpdateListener
    {
        public const int OutgoingQueueMaxCapacity = 1024;
        public const int ExchangeOutBucketSize = 64;
        public static readonly TimeSpan UploadRetryInterval = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan EndpointChangeQueryInterval = TimeSpan.FromSeconds(60);

        private static readonly ILog Logger = LogManager.GetLogger<MonitorDataExchange>();
        private readonly Thread _exchangeThread;
        private readonly IHealthMonitorRegistry _registry;
        private readonly IHealthMonitorExchangeClient _exchangeClient;
        private readonly IMonitorableEndpointRegistry _monitorableEndpointRegistry;
        private readonly OutgoingQueue<EndpointHealthUpdate> _outgoingQueue = new OutgoingQueue<EndpointHealthUpdate>(OutgoingQueueMaxCapacity);
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public MonitorDataExchange(IHealthMonitorRegistry registry, IHealthMonitorExchangeClient exchangeClient, IMonitorableEndpointRegistry monitorableEndpointRegistry)
        {
            _registry = registry;
            _exchangeClient = exchangeClient;
            _monitorableEndpointRegistry = monitorableEndpointRegistry;
            _exchangeThread = new Thread(StartExchange) { Name = "Exchange" };
            _exchangeThread.Start();
        }

        private void StartExchange()
        {
            Task.WaitAll(ExchangeOut(), ExchangeIn());
        }

        private async Task ExchangeOut()
        {
            await Task.Yield();
            await RegisterMonitors();

            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    var bucket = _outgoingQueue.Dequeue(ExchangeOutBucketSize, UploadRetryInterval, _cancellation.Token);
                    if (bucket.Length > 0)
                        await UploadChangesAsync(bucket);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to exchange data: {e.Message}", e);
                    await Task.Delay(UploadRetryInterval, _cancellation.Token);
                }
            }
        }

        private async Task UploadChangesAsync(EndpointHealthUpdate[] bucket)
        {
            while (true)
            {
                try
                {
                    await _exchangeClient.UploadHealthAsync(bucket, _cancellation.Token);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to upload endpoint updates: {e.Message}", e);
                    await Task.Delay(UploadRetryInterval, _cancellation.Token);
                }
            }
        }

        private async Task ExchangeIn()
        {
            await Task.Yield();
            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    _monitorableEndpointRegistry.UpdateEndpoints(await _exchangeClient.GetEndpointIdentitiesAsync(_cancellation.Token));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to query changes: {e.Message}", e);
                }

                await Task.Delay(EndpointChangeQueryInterval, _cancellation.Token);
            }
        }

        private async Task RegisterMonitors()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    //TODO: create a concept of monitor instance
                    await _exchangeClient.RegisterMonitorsAsync(_registry.MonitorTypes, _cancellation.Token);
                    return;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to register monitors: {e.Message}", e);
                    await Task.Delay(UploadRetryInterval, _cancellation.Token);
                }
            }
        }

        public void UpdateHealth(Guid endpointId, EndpointHealth endpointHealth)
        {
            _outgoingQueue.Enqueue(new EndpointHealthUpdate(endpointId, endpointHealth));
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _exchangeThread.Join();
        }
    }
}
