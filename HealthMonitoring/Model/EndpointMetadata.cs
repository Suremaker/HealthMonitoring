namespace HealthMonitoring.Model
{
    public class EndpointMetadata
    {
        public EndpointMetadata(string name, string group)
        {
            Name = name;
            Group = group;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
    }
}