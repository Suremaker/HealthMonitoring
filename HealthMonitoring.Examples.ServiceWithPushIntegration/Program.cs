using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient;
using HealthMonitoring.Integration.PushClient.Monitoring;

namespace HealthMonitoring.Examples.ServiceWithPushIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StartHealthMonitorIntegration())
            {
                Console.ReadKey();
            }
        }

        private static IEndpointHealthNotifier StartHealthMonitorIntegration()
        {
            return HealthMonitorPushClient.UsingHealthMonitor(ConfigurationManager.AppSettings["HealthMonitorUrl"])
                .DefineEndpoint(builder => builder
                    .DefineGroup("Examples")
                    .DefineName("Service With Push Integration")
                    .DefineTags("example")
                    .DefineAddress("ServiceWithPushIntegration_node1")
                    .DefineAuthenticationToken("12345678"))
                .WithHealthCheck(new HealthChecker())
                .StartHealthNotifier();
        }
    }

    internal class HealthChecker : AbstractHealthChecker
    {
        protected override Task<HealthStatus> OnHealthCheckAsync(Dictionary<string, string> details, CancellationToken cancellationToken)
        {
            return Task.FromResult(HealthStatus.Healthy);
        }
    }
}
