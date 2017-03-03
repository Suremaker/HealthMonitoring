using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient.Helpers
{
    public class BackOffPlan
    {
        public TimeSpan? RetryInterval { get; set; }

        public bool ShouldLog { get; set; }
    }
}
