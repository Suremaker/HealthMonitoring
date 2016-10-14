using System;
using System.Configuration;
using System.Threading.Tasks;
using HealthMonitoring.Integration.PushClient;
using HealthMonitoring.Integration.PushClient.Monitoring;
using HealthMonitoring.Monitors;

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
                    .DefineAddress("ServiceWithPushIntegration_node1"))
                .WithHealthCheckMethod(token => Task.FromResult(new HealthInfo(HealthStatus.Healthy)))
                .StartHealthNotifier();
        }
    }
}
