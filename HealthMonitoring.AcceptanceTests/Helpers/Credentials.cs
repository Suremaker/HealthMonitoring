using System;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    class Credentials
    {
        public Guid MonitorId { get; set; }
        public string PrivateToken { get; set; }

        public Credentials(Guid id, string token)
        {
            MonitorId = id;
            PrivateToken = token;
        }
    }
}
