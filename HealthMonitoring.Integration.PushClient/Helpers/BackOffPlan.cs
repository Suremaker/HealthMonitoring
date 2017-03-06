using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public class BackOffPlan
    {
        public TimeSpan? RetryInterval { get; private set; }

        public bool ShouldLog { get; private set; }

        public BackOffPlan(TimeSpan? retryInterval, bool shouldLog)
        {
            RetryInterval = retryInterval;
            ShouldLog = shouldLog;
        }
    }
}
