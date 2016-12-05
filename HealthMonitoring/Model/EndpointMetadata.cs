using System;
using System.Linq;

namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public const string DefaultMonitorTag = "default";
        public EndpointMetadata(string name, string group, string[] tags, string monitorTag, DateTime registeredOnUtc, DateTime registrationUpdatedOnUtc)
        {
            if (string.IsNullOrWhiteSpace(monitorTag))
                throw new ArgumentException("MonitorTag cannot be null or empty");

            Name = name;
            Group = group;
            Tags = tags?.Distinct().ToArray() ?? new string[0];
            RegisteredOnUtc = registeredOnUtc;
            RegistrationUpdatedOnUtc = registrationUpdatedOnUtc;
            MonitorTag = monitorTag;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string[] Tags { get; private set; }
        public DateTime RegisteredOnUtc { get; private set; }
        public DateTime RegistrationUpdatedOnUtc { get; private set; }
        public string MonitorTag { get; private set; }
    }
}