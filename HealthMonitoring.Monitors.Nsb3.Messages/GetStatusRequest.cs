using System;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3.Messages
{
    [Serializable]
    public class GetStatusRequest : ICommand
    {
        public Guid RequestId { get; set; }
    }
}