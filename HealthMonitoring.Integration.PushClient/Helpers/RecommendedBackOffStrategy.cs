using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public class RecommendedBackOffStrategy : IBackOffStategy
    {
        private readonly TimeSpan _maxRepeatDelay;

        private readonly TimeSpan _intervalMultiplier;

        private readonly TimeSpan _initialInterval = TimeSpan.FromSeconds(1);

        public RecommendedBackOffStrategy()
            : this(TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(2))
        {
        }

        public RecommendedBackOffStrategy(TimeSpan maxRepeatDelay, TimeSpan intervalMultiplier)
        {
            _maxRepeatDelay = maxRepeatDelay;
            _intervalMultiplier = intervalMultiplier;
        }
        

        public virtual Task<TimeSpan?> NextInterval(TimeSpan? currentInterval, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var suggestedInterval = currentInterval?.TotalSeconds * _intervalMultiplier.TotalSeconds ?? _initialInterval.TotalSeconds;

                TimeSpan newInterval = TimeSpan.FromSeconds(Math.Min(_maxRepeatDelay.TotalSeconds, suggestedInterval));
                
                return Task.FromResult(newInterval as TimeSpan?);
            }

            return Task.FromResult<TimeSpan?>(null);
        }


        public virtual Task<bool> RecordLog(TimeSpan? currentInterval, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public virtual async Task<BackOffPlan> GetCurrent(TimeSpan? currentInterval, CancellationToken cancellationToken)
        {
            TimeSpan? newInterval = await NextInterval(currentInterval, cancellationToken);

            bool shouldLog = await RecordLog(currentInterval, cancellationToken);

            var backOffPlan = new BackOffPlan(newInterval, shouldLog);

            return backOffPlan;
        }
    }
}
