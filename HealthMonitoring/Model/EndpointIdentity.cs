using System;

namespace HealthMonitoring.Model
{
    public class EndpointIdentity
    {
        public Guid Id { get; }
        public string MonitorType { get; }
        public string Address { get; }
        public EndpointIdentity(Guid id, string monitorType, string address) 
        {
            if (monitorType == null)
                throw new ArgumentNullException(nameof(monitorType));
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            Id = id;
            MonitorType = monitorType;
            Address = address;
        }

        public override string ToString()
        {
            return $"{MonitorType}: {Address}";
        }

        public string GetOwnNaturalKey()
        {
            return CreateNaturalKey(MonitorType, Address);
        }

        public static string CreateNaturalKey(string monitorType, string address)
        {
            return $"{monitorType}|{address?.ToLowerInvariant()}";
        }
    }
}