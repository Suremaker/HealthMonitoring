using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public interface IBackOffStategy
    {

        Task<TimeSpan?> NextInterval(TimeSpan? currentInterval, CancellationToken cancellationToken);

        Task<bool> RecordLog(TimeSpan? currentInterval, CancellationToken cancellationToken);

        Task<BackOffPlan> GetCurrent(TimeSpan? currentInterval, CancellationToken cancellationToken);

    }
}
