using System;
using System.Collections.Concurrent;
using System.Threading;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Management.Core.Repositories;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Management.Core
{
    public class EndpointStatsManager : IEndpointStatsManager, IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger<EndpointStatsManager>();
        private readonly IEndpointStatsRepository _repository;
        private readonly IMonitorSettings _settings;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly Thread _cleanerThread;
        private readonly Thread _writerThread;
        private readonly IEndpointMetricsForwarderCoordinator _metricsForwarderCoordinator;
        private readonly BlockingCollection<Tuple<EndpointIdentity, EndpointMetadata, EndpointHealth>> _statsQueue = new BlockingCollection<Tuple<EndpointIdentity, EndpointMetadata, EndpointHealth>>();

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
                _logger.InfoFormat("Starting stats writer thread...");
                while (true)
                {
                    Tuple<EndpointIdentity, EndpointMetadata, EndpointHealth> item;
                    if (_statsQueue.TryTake(out item, TimeSpan.FromMilliseconds(250)))
                    {
                        InsertStatistics(item.Item1.Id, item.Item3);
                        _metricsForwarderCoordinator.HandleMetricsForwarding(item.Item1, item.Item2, item.Item3);
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException)
            {
                _logger.InfoFormat("Exiting stats writer thread...");
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
                _logger.ErrorFormat("Unable to insert endpoint statistics: {0}", e.ToString());
            }
        }

        private void Clean()
        {
            try
            {
                _logger.InfoFormat("Starting stats cleaner thread...");
                while (true)
                {
                    DeleteOldStatistics();
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            }
            catch (ThreadInterruptedException)
            {
                _logger.InfoFormat("Exiting stats cleaning thread...");
            }
        }

        private void DeleteOldStatistics()
        {
            try
            {
                var date = _timeCoordinator.UtcNow.Subtract(_settings.StatsHistoryMaxAge);
                _logger.InfoFormat("Deleting older stats than {0}", date);
                _repository.DeleteStatisticsOlderThan(date);
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Unable to delete old statistics: {0}", e.ToString());
            }
        }

        public void RecordEndpointStatistics(EndpointIdentity identity, EndpointMetadata metadata, EndpointHealth stats)
        {
            _statsQueue.Add(Tuple.Create(identity, metadata, stats));
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