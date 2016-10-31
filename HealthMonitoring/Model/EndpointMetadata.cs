using System;
using System.Linq;

namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public EndpointMetadata(string name, string group, string[] tags, DateTime? firstTimeRegistered = null, DateTime? lastTimeRegistrationUpdated = null)
        {
            Name = name;
            Group = group;
            FirstTimeRegistered = firstTimeRegistered;
            LastTimeRegistrationUpdated = lastTimeRegistrationUpdated;
            Tags = tags?.Distinct().ToArray() ?? new string[0];
        }

        public void SetFirstRegistrationTime(DateTime date)
        {
            FirstTimeRegistered = date;
        }

        public void SetLastRegistrationUpdateTime(DateTime date)
        {
            LastTimeRegistrationUpdated = date;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string[] Tags { get; private set; }
        public DateTime? FirstTimeRegistered { get; private set; }
        public DateTime? LastTimeRegistrationUpdated { get; private set; }
    }
}