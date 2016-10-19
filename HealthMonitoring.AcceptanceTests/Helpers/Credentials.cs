using System;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    class Credentials
    {
        public Guid Id { get; set; }
        public string PrivateToken { get; set; }

        public Credentials(Guid id, string token)
        {
            Id = id;
            PrivateToken = token;
        }
    }
}
