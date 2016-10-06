using System.Linq;

namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public EndpointMetadata(string name, string group, string[] tags)
        {
            Name = name;
            Group = group;
            Tags = tags?.Distinct().ToArray() ?? new string[0];
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
        public string[] Tags { get; private set; }
    }
}