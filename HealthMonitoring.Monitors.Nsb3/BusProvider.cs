using HealthMonitoring.Monitors.Nsb3.Messages;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace HealthMonitoring.Monitors.Nsb3
{
    internal static class BusProvider
    {
        public static IBus Create()
        {
            return Configure.With(typeof(BusProvider).Assembly,typeof(GetStatusRequest).Assembly)
                .Log4Net()
                .DefineEndpointName("HealthMonitoring.Monitors.Nsb3")
                .DefaultBuilder()
                .MsmqTransport()
                .InMemorySagaPersister()
                .InMemorySubscriptionStorage()
                .DisableRavenInstall()
                .DisableSecondLevelRetries()
                .DisableTimeoutManager()
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
                ErrorQueue = "HealthMonitoring.Monitors.Nsb3.error"
            };
        }
    }
}