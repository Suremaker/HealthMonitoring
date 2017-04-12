using System;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb6.Messages;
using NServiceBus;

namespace HealthMonitoring.Monitors.Nsb6.RabbitMq
{
    public class GetStatusResponseHandler : IHandleMessages<GetStatusResponse>
    {
        public static event Action<GetStatusResponse> OnResponse;

        public Task Handle(GetStatusResponse message, IMessageHandlerContext context)
        {
            OnResponse?.Invoke(message);
            return Task.CompletedTask;
        }
    }
}