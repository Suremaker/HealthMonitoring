using System;
using System.Threading;
using Common.Logging;
using HealthMonitoring.Configuration;
using HealthMonitoring.Model;

namespace HealthMonitoring
{
    public class EndpointStatsManager : IEndpointStatsManager, IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger<EndpointStatsManager>();
        private readonly IEndpointStatsRepository _repository;
        private readonly IMonitorSettings _settings;
        private readonly Thread _cleanerThread;

        public EndpointStatsManager(IEndpointStatsRepository repository, IMonitorSettings settings)
        {
            _repository = repository;
            _settings = settings;
            _cleanerThread = new Thread(Clean) { IsBackground = true, Name = "StatsCleaner" };
            _cleanerThread.Start();
        }

        private void Clean()
        {
            try
            {
                while (true)
                {
                    var date = DateTime.UtcNow.Subtract(_settings.StatsHistoryMaxAge);
                    _logger.DebugFormat("Deleting older stats than {0}", date);
                    _repository.DeleteStatisticsOlderThan(date);
                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            }
            catch (ThreadInterruptedException) { }
        }

        public void RecordEndpointStatistics(Guid endpointId, EndpointHealth stats)
        {
            _repository.InsertEndpointStatistics(endpointId, stats);
        }

        public void Dispose()
        {
            _cleanerThread.Interrupt();
            _cleanerThread.Join();
        }
    }
}