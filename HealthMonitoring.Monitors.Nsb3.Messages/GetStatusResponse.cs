using System;
using System.Collections.Generic;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3.Messages
{
    [Serializable]
    public class GetStatusResponse : IMessage
    {
        public Guid RequestId { get; set; }
        public Dictionary<string, string> Details { get; set; }
    }
}
