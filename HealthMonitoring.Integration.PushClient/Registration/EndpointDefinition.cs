namespace HealthMonitoring.Integration.PushClient.Registration
{
    public class EndpointDefinition
    {
        public string Address { get; }
        public string GroupName { get; }
        public string EndpointName { get; }
        public string[] Tags { get; }
        public string Password { get;  }

        public EndpointDefinition(string address, string groupName, string endpointName, string[] tags, string password)
        {
            Address = address;
            GroupName = groupName;
            EndpointName = endpointName;
            Tags = tags;
            Password = password;
        }
    }
}