using System;
using System.Linq;

namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public EndpointMetadata(string name, string group, string[] tags, DateTime firstTimeRegistered, DateTime lastTimeRegistrationUpdated)
        {
            Name = name;
            Group = group;
            Tags = tags?.Distinct().ToArray() ?? new string[0];
            FirstTimeRegistered = firstTimeRegistered;
            LastTimeRegistrationUpdated = lastTimeRegistrationUpdated;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string[] Tags { get; private set; }
        public DateTime FirstTimeRegistered { get; private set; }
        public DateTime LastTimeRegistrationUpdated { get; private set; }
    }
}