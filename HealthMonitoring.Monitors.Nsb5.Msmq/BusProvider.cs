using System;
using System.Messaging;
using HealthMonitoring.Monitors.Nsb5.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;

namespace HealthMonitoring.Monitors.Nsb5.Msmq
{
    internal static class QueueHelper
    {
        public static void CreateQueue(string queueName)
        {
            var queueFullName = ".\\private$\\" + queueName;
            if (!MessageQueue.Exists(queueFullName))
            {
                var queue = MessageQueue.Create(queueFullName, true);
                queue.SetPermissions("Everyone", MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow);
            }
        }
    }

    internal static class BusProvider
    {
        public const string QueueName = "HealthMonitoring.Monitors.Nsb5.Msmq";
        public const string ErrorQueueName = QueueName + ".Errors";

        public static IBus Create(TimeSpan timeout)
        {
            QueueHelper.CreateQueue(QueueName);
            QueueHelper.CreateQueue(ErrorQueueName);

            var busConfiguration = new BusConfiguration();
            busConfiguration.AssembliesToScan(typeof(GetStatusRequest).Assembly, typeof(BusProvider).Assembly);
            busConfiguration.EndpointName(QueueName);
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UsePersistence<InMemoryPersistence>();

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