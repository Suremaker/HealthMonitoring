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
        Task<TimeSpan?> Apply(TimeSpan? currentInterval, CancellationToken cancellationToken);
    }
}
