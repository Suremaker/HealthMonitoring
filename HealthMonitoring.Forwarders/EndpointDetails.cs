using System;

namespace HealthMonitoring.Forwarders
{
    public class EndpointDetails
    {
        public Guid EndpointId { get; private set; }
        public string Group { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string MonitorType { get; private set; }

        public EndpointDetails(Guid id, string group, string name, string address, string monitorType)
        {
            EndpointId = id;
            Group = group;
            Name = name;
            Address = address;
            MonitorType = monitorType;
        }
    }
}