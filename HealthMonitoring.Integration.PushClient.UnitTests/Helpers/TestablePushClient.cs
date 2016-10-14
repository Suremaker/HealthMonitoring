using HealthMonitoring.Integration.PushClient.Client;
using HealthMonitoring.Integration.PushClient.Helpers;

namespace HealthMonitoring.Integration.PushClient.UnitTests.Helpers
{
    class TestablePushClient : HealthMonitorPushClient
    {
        public TestablePushClient(IHealthMonitorClient client, ITimeCoordinator timeCoordinator) : base(client, timeCoordinator)
        {
        }
    }
}