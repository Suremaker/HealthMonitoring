using System;
using System.Collections.Generic;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;

namespace HealthMonitoring.SampleNsb5Host.Rabbitmq
{
    class Program
    {
        static void Main(string[] args)
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.AssembliesToScan(typeof(GetStatusRequest).Assembly, typeof(Handler).Assembly, typeof(RabbitMQTransport).Assembly);
            busConfiguration.EndpointName("HealthMonitoring.SampleNsb5Host.Rabbitmq");
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UseTransport<RabbitMQTransport>().ConnectionStringName("RabbitMqConnectionString");
            busConfiguration.UsePersistence<InMemoryPersistence>();

            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Warn);

            using (var bus = Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("\r\nBus created and configured; press any key to stop program\r\n");
                Console.ReadKey();
            }
        }

        public class Handler : IHandleMessages<GetStatusRequest>
        {
            private readonly IBus _bus;

            public Handler(IBus bus)
            {
                _bus = bus;
            }

            public void Handle(GetStatusRequest message)
            {
                Console.WriteLine("Received: {0}", message.RequestId);
                var details = new Dictionary<string, string> { { "Machine", "localhost" }, { "Version", "1.0.0.0" } };
                _bus.Reply(new GetStatusResponse { RequestId = message.RequestId, Details = details });
            }
        }

        class ConfigErrorQueue : IProvideConfiguration<MessageForwardingInCaseOfFaultConfig>
        {
            public MessageForwardingInCaseOfFaultConfig GetConfiguration()
            {
                return new MessageForwardingInCaseOfFaultConfig
                {
                    ErrorQueue = "HealthMonitoring.SampleNsb5Host.Rabbitmq.error"
                };
            }
        }
    }
}
