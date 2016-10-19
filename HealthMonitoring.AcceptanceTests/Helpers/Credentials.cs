using System;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    class Credentials
    {
        public Guid Id { get; set; }
        public string Password { get; set; }

        public Credentials(Guid id, string password)
        {
            Id = id;
            Password = password;
        }
    }
}
