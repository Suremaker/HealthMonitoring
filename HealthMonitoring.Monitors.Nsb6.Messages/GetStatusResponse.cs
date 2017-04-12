using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb6.Messages
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
                details = string.Join(", ", Details.Select(d => $"{d.Key}={d.Value}"));

            return $"{GetType()} - RequestId={RequestId}, Details={details}";
        }
    }
}
