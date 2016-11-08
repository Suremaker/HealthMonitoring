using System;
using System.Linq;

namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public EndpointMetadata(string name, string group, string[] tags, DateTime registeredOnUtc, DateTime registrationUpdatedOnUtc)
        {
            Name = name;
            Group = group;
            Tags = tags?.Distinct().ToArray() ?? new string[0];
            RegisteredOnUtc = registeredOnUtc;
            RegistrationUpdatedOnUtc = registrationUpdatedOnUtc;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string[] Tags { get; private set; }
        public DateTime RegisteredOnUtc { get; private set; }
        public DateTime RegistrationUpdatedOnUtc { get; private set; }
    }
}