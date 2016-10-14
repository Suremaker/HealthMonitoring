using System.Collections.Generic;

namespace HealthMonitoring.Integration.PushClient.Registration
{
    public interface IEndpointDefintionBuilder
    {
        IEndpointDefintionBuilder DefineGroup(string groupName);
        IEndpointDefintionBuilder DefineName(string endpointName);
        IEndpointDefintionBuilder DefineTags(params string[] tags);
        IEndpointDefintionBuilder DefineAddress(string endpointUniqueName);
        IEndpointDefintionBuilder DefineAddress(string host, string endpointUniqueName);
    }
}