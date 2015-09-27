using System;
using System.Messaging;
using HealthMonitoring.Monitors.Nsb3.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace HealthMonitoring.Monitors.Nsb3
{
    internal static class QueueHelper
    {
        public static void CreateQueue(string queueName)
        {
            var queue = ".\\private$\\" + queueName;
            if (!MessageQueue.Exists(queue))
                MessageQueue.Create(queue, true);
        }
    }

    internal static class BusProvider
    {
        public const string QueueName = "HealthMonitoring.Monitors.Nsb3";
        public const string ErrorQueueName = QueueName + ".Errors";

        public static IBus Create(TimeSpan timeout)
        {
            QueueHelper.CreateQueue(QueueName);
            QueueHelper.CreateQueue(ErrorQueueName);

            return Configure.With(typeof(BusProvider).Assembly, typeof(GetStatusRequest).Assembly)
                .DefineEndpointName(QueueName)
                .DefaultBuilder()
                .MsmqTransport()
                .IsTransactional(true)
                .InMemorySagaPersister()
                .InMemorySubscriptionStorage()
                .DisableRavenInstall()
                .DisableSecondLevelRetries()
                .DisableTimeoutManager()
                .DefiningTimeToBeReceivedAs(type => timeout)
                .UnicastBus()
                .CreateBus()
                .Start();
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