using System;
using System.Linq;
using System.Threading;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.Queueing;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Management.Core
{
    public class EndpointStatsManager : IEndpointStatsManager, IDisposable
    {
        private const int MaxStatsQueueSize = 10000;
        private static readonly ILog Logger = LogManager.GetLogger<EndpointStatsManager>();
        private readonly IEndpointStatsRepository _repository;
        private readonly IMonitorSettings _settings;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly Thread _cleanerThread;
        private readonly Thread _writerThread;
        private readonly IEndpointMetricsForwarderCoordinator _metricsForwarderCoordinator;
        private readonly OutgoingQueue<Tuple<EndpointIdentity, EndpointMetadata, EndpointHealth>> _statsQueue = new OutgoingQueue<Tuple<EndpointIdentity, EndpointMetadata, EndpointHealth>>(MaxStatsQueueSize);
        private const int BatchSize = 1024;

        public EndpointStatsManager(IEndpointStatsRepository repository, IMonitorSettings settings, ITimeCoordinator timeCoordinator, IEndpointMetricsForwarderCoordinator metricsForwarderCoordinator)
        {
            _repository = repository;
            _settings = settings;
            _timeCoordinator = timeCoordinator;
            _metricsForwarderCoordinator = metricsForwarderCoordinator;

            _cleanerThread = new Thread(Clean) { Name = "StatsCleaner" };
            _cleanerThread.Start();
            _writerThread = new Thread(WriteStats) { Name = "StatsWriter" };
            _writerThread.Start();
        }

        private void WriteStats()
        {
            try
            {
                Logger.Info("Starting stats writer thread...");
                while (true)
                {
                    var item = _statsQueue.Dequeue(1, TimeSpan.FromMilliseconds(250), CancellationToken.None).FirstOrDefault();
                    if (item == null)
                        continue;

                    InsertStatistics(item.Item1.Id, item.Item3);
                    _metricsForwarderCoordinator.HandleMetricsForwarding(item.Item1, item.Item2, item.Item3);
                }
            }
            catch (ThreadInterruptedException)
            {
                Logger.Info("Exiting stats writer thread...");
            }
        }

        private void InsertStatistics(Guid id, EndpointHealth health)
        {
            try
            {
                _repository.InsertEndpointStatistics(id, health);
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to insert endpoint statistics: {e}");
            }
        }

        private void Clean()
        {
            try
            {
                Logger.Info("Starting stats cleaner thread...");
                while (true)
                {
                    DeleteOldStatistics();
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            }
            catch (ThreadInterruptedException)
            {
                Logger.Info("Exiting stats cleaning thread...");
            }
        }

        private void DeleteOldStatistics()
        {
            long total = 0;
            try
            {
                var date = _timeCoordinator.UtcNow.Subtract(_settings.StatsHistoryMaxAge);
                Logger.Info($"Deleting stats older than {date}");
                long current;
                while ((current = _repository.DeleteStatisticsOlderThan(date, BatchSize)) > 0)
                    total += current;
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to delete old statistics: {e}");
            }
            Logger.Info($"Deleted {total} old stats.");
        }

        public void RecordEndpointStatistics(EndpointIdentity identity, EndpointMetadata metadata, EndpointHealth stats)
        {
            _statsQueue.Enqueue(Tuple.Create(identity, metadata, stats));
        }

        public void Dispose()
        {
            _cleanerThread.Interrupt();
            _writerThread.Interrupt();
            _cleanerThread.Join();
            _writerThread.Join();
        }
    }
}
