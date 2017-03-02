using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.TimeManagement;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public class DefaultBackOffStrategy : IBackOffStategy
    {
        private const int MaxRepeatDelayInSecs = 120;

        private const int IntervalIncrementInSecs = 1;

        public Task<TimeSpan?> Apply(TimeSpan? currentInterval, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                currentInterval = currentInterval ?? TimeSpan.FromSeconds(1);

                return Task.FromResult(
                    TimeSpan.FromSeconds(Math.Min(MaxRepeatDelayInSecs, currentInterval.Value.Seconds)) + TimeSpan.FromSeconds(IntervalIncrementInSecs) 
                    as TimeSpan?);
            }

            return Task.FromResult<TimeSpan?>(null);
        }
    }
}
