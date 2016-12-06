using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange.Client;
using HealthMonitoring.Monitors.Core.Registers;
using HealthMonitoring.Queueing;

namespace HealthMonitoring.Monitors.Core.Exchange
{
    public class MonitorDataExchange : IDisposable, IEndpointHealthUpdateListener
    {
        private static readonly ILog Logger = LogManager.GetLogger<MonitorDataExchange>();
        private readonly Thread _exchangeThread;
        private readonly IHealthMonitorRegistry _registry;
        private readonly IHealthMonitorExchangeClient _exchangeClient;
        private readonly IMonitorableEndpointRegistry _monitorableEndpointRegistry;
        private readonly OutgoingQueue<EndpointHealthUpdate> _outgoingQueue;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly DataExchangeConfig _config;

        public MonitorDataExchange(IHealthMonitorRegistry registry, IHealthMonitorExchangeClient exchangeClient, IMonitorableEndpointRegistry monitorableEndpointRegistry, DataExchangeConfig config)
        {
            _config = config;
            _registry = registry;
            _exchangeClient = exchangeClient;
            _monitorableEndpointRegistry = monitorableEndpointRegistry;
            _outgoingQueue = new OutgoingQueue<EndpointHealthUpdate>(_config.OutgoingQueueMaxCapacity);
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
                    var bucket = _outgoingQueue.Dequeue(_config.ExchangeOutBucketSize, _config.UploadRetryInterval, _cancellation.Token);
                    if (bucket.Length > 0)
                        await UploadChangesAsync(bucket);
                }
                catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
                {
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to exchange data: {e.Message}", e);
                    await Task.Delay(_config.UploadRetryInterval, _cancellation.Token);
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
                catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to upload endpoint updates: {e.Message}", e);
                    await Task.Delay(_config.UploadRetryInterval, _cancellation.Token);
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
                    _monitorableEndpointRegistry.UpdateEndpoints(await _exchangeClient.GetEndpointIdentitiesAsync(_config.MonitorTag, _cancellation.Token));
                }
                catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to query changes: {e.Message}", e);
                }

                await DelayNoThrow(_config.EndpointChangeQueryInterval);
            }
        }

        private async Task RegisterMonitors()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    await _exchangeClient.RegisterMonitorsAsync(_registry.MonitorTypes, _cancellation.Token);
                    return;
                }
                catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
                {
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to register monitors: {e.Message}", e);
                    await DelayNoThrow(_config.UploadRetryInterval);
                }
            }
        }

        private async Task DelayNoThrow(TimeSpan interval)
        {
            try
            {
                await Task.Delay(interval, _cancellation.Token);
            }
            catch (OperationCanceledException) { }
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
