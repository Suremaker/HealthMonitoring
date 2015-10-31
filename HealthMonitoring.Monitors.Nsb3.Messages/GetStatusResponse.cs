using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3.Messages
{
    [Serializable]
    public class GetStatusResponse : IMessage
    {
        public Guid RequestId { get; set; }
        public Dictionary<string, string> Details { get; set; }

        public override string ToString()
        {
            var details = string.Empty;
            if (Details != null)
                details = string.Join(", ", Details.Select(d => string.Format("{0}={1}", d.Key, d.Value)));

            return string.Format("{0} - RequestId={1}, Details={2}", GetType(), RequestId, details);
        }
    }
}
