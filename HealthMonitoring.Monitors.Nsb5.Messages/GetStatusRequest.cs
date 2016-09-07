using System;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb5.Messages
{
    [Serializable]
    public class GetStatusRequest : ICommand
    {
        public Guid RequestId { get; set; }

        public override string ToString()
        {
            return $"{GetType()} - RequestId={RequestId}";
        }
    }
}