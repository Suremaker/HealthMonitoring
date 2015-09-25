using System;
using HealthMonitoring.Monitors.Nsb3.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb3
{
    public class GetStatusResponseHandler : IHandleMessages<GetStatusResponse>
    {
        public static event Action<GetStatusResponse> OnResponse;
        public void Handle(GetStatusResponse message)
        {
            var handler = OnResponse;
            if (handler != null)
                handler.Invoke(message);
        }
    }
}