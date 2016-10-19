namespace HealthMonitoring.Integration.PushClient.Registration
{
    public class EndpointDefinition
    {
        public string Address { get; }
        public string GroupName { get; }
        public string EndpointName { get; }
        public string[] Tags { get; }
        public string AuthenticationToken { get;  }

        public EndpointDefinition(string address, string groupName, string endpointName, string[] tags, string authenticationToken)
        {
            Address = address;
            GroupName = groupName;
            EndpointName = endpointName;
            Tags = tags;
            AuthenticationToken = authenticationToken;
        }
    }
}