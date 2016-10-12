using System;

namespace HealthMonitoring.AcceptanceTests.Helpers.Entities
{
    class RegistrationResponse
    {
        public Guid Id { get; set; }
        public string Token { get; set; }

        public RegistrationResponse(Guid id, string token)
        {
            Id = id;
            Token = token;
        }
    }
}
