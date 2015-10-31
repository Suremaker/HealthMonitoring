using System;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3.Messages
{
    [Serializable]
    public class GetStatusRequest : ICommand
    {
        public Guid RequestId { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - RequestId={1}", GetType(), RequestId);
        }
    }
}