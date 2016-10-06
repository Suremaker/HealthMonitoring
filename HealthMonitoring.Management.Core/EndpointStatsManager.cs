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
        private readonly BlockingCollection<Tuple<Guid, EndpointHealth>> _statsQueue = new BlockingCollection<Tuple<Guid, EndpointHealth>>();

        public EndpointStatsManager(IEndpointStatsRepository repository, IMonitorSettings settings, ITimeCoordinator timeCoordinator)
        {
            _repository = repository;
            _settings = settings;
            _timeCoordinator = timeCoordinator;
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
                    Tuple<Guid, EndpointHealth> item;
                    if (_statsQueue.TryTake(out item, TimeSpan.FromMilliseconds(250)))
                        InsertStatistics(item);

                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException)
            {
                _logger.InfoFormat("Exiting stats writer thread...");
            }
        }

        private void InsertStatistics(Tuple<Guid, EndpointHealth> item)
        {
            try
            {
                _repository.InsertEndpointStatistics(item.Item1, item.Item2);
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

        public void RecordEndpointStatistics(Guid endpointId, EndpointHealth stats)
        {
            _statsQueue.Add(Tuple.Create(endpointId, stats));
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