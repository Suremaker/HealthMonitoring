using System;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;

namespace HealthMonitoring.Monitors.Nsb5.Rabbitmq
{
    internal static class BusProvider
    {
        public const string QueueName = "HealthMonitoring.Monitors.Nsb5.Rabbitmq";
        public const string ErrorQueueName = QueueName + ".Errors";

        public static IBus Create(TimeSpan timeout)
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.AssembliesToScan(typeof(GetStatusRequest).Assembly, typeof(BusProvider).Assembly, typeof(RabbitMQTransport).Assembly);
            busConfiguration.EndpointName(QueueName);
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.UseTransport<RabbitMQTransport>().ConnectionStringName("RabbitMqConnectionString");

            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Warn);

            var conventions = busConfiguration.Conventions();
            conventions.DefiningTimeToBeReceivedAs(type => type == typeof(GetStatusRequest) ? timeout : TimeSpan.MaxValue);

            return Bus.Create(busConfiguration).Start();
        }
    }

    class ConfigErrorQueue : IProvideConfiguration<MessageForwardingInCaseOfFaultConfig>
    {
        public MessageForwardingInCaseOfFaultConfig GetConfiguration()
        {
            return new MessageForwardingInCaseOfFaultConfig
            {
                ErrorQueue = BusProvider.ErrorQueueName
            };
        }
    }

}