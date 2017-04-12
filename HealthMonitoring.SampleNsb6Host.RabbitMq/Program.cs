using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthMonitoring.Monitors.Nsb6.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace HealthMonitoring.SampleNsb6Host.RabbitMq
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpointConfiguration = new EndpointConfiguration("HealthMonitoring.SampleNsb6Host.RabbitMq");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseTransport<RabbitMQTransport>().ConnectionStringName("RabbitMqConnectionString");
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("HealthMonitoring.SampleNsb6Host.RabbitMq.error");

            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Info);

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            Console.ReadKey();
            endpoint.Stop().GetAwaiter().GetResult();
        }

        public class Handler : IHandleMessages<GetStatusRequest>
        {
            public async Task Handle(GetStatusRequest message, IMessageHandlerContext context)
            {
                Console.WriteLine("Received RequestId={0}", message.RequestId);
                var details = new Dictionary<string, string> { { "Machine", "localhost" }, { "Version", "1.0.0.0" } };
                await context.Reply(new GetStatusResponse { RequestId = message.RequestId, Details = details });
            }
        }
    }
}
