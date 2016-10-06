using System;
using System.Collections.Generic;
using System.Messaging;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace HealthMonitoring.SampleNsb5Host.Msmq
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateQueue();

            var busConfiguration = new BusConfiguration();
            busConfiguration.AssembliesToScan(typeof(GetStatusRequest).Assembly, typeof(Handler).Assembly);
            busConfiguration.EndpointName("HealthMonitoring.SampleNsb5Host.Msmq");
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UseTransport<MsmqTransport>();

            busConfiguration.UsePersistence<InMemoryPersistence>();

            using (var bus = Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("\r\nBus created and configured; press any key to stop program\r\n");
                Console.ReadKey();
            }
        }

        private static void CreateQueue()
        {
            var queue = ".\\private$\\HealthMonitoring.SampleNsb5Host.Msmq";
            if (!MessageQueue.Exists(queue))
                MessageQueue.Create(queue, true);
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
                    ErrorQueue = "HealthMonitoring.SampleNsb5Host.Msmq.error"
                };
            }
        }
    }
}
