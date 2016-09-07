using System;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb5.Rabbitmq
{
    public class GetStatusResponseHandler : IHandleMessages<GetStatusResponse>
    {
        public static event Action<GetStatusResponse> OnResponse;
        public void Handle(GetStatusResponse message)
        {
            OnResponse?.Invoke(message);
        }
    }
}