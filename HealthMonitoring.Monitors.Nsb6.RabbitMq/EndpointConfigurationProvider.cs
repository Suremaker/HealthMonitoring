using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HealthMonitoring.Monitors.Nsb6.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace HealthMonitoring.Monitors.Nsb6.RabbitMq
{
    internal static class EndpointConfigurationProvider
    {
        private const string QueueName = "HealthMonitoring.Monitors.Nsb6.RabbitMq";
        private const string ErrorQueueName = QueueName + ".Errors";

        public static EndpointConfiguration BuildEndpointConfiguration(TimeSpan timeout)
        {
            var endpointConfiguration = new EndpointConfiguration(QueueName);

            var assemblies = new[]
            {
                typeof(GetStatusRequest).Assembly,
                typeof(EndpointConfigurationProvider).Assembly,
                typeof(RabbitMQTransport).Assembly,
                typeof(Bus).Assembly,
                typeof(RabbitMQTransport).Assembly
            };
            var assemblyScannerConfiguration = endpointConfiguration.AssemblyScanner();
            assemblyScannerConfiguration.ScanAssembliesInNestedDirectories = true;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var excludedAssemblyNames = Directory.EnumerateFiles(baseDirectory, "*.dll", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .Except(assemblies.Select(a => Path.GetFileName(a.Location)))
                .ToArray();
            assemblyScannerConfiguration.ExcludeAssemblies(excludedAssemblyNames);

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseTransport<RabbitMQTransport>().ConnectionStringName("RabbitMqConnectionString");

            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Warn);

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningTimeToBeReceivedAs(type => type == typeof(GetStatusRequest) ? timeout : TimeSpan.MaxValue);

            endpointConfiguration.SendFailedMessagesTo(ErrorQueueName);

            return endpointConfiguration;
        }
    }
}