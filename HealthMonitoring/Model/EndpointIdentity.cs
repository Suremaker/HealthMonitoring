using System;

namespace HealthMonitoring.Model
{
    public class EndpointIdentity
    {
        public Guid Id { get; }
        public string MonitorType { get; }
        public string Address { get; }
        public string PrivateToken { get; set; }

        public EndpointIdentity(Guid id, string monitorType, string address, string privateToken = null) 
        {
            if (monitorType == null)
                throw new ArgumentNullException(nameof(monitorType));
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            Id = id;
            MonitorType = monitorType;
            Address = address;
            PrivateToken = privateToken;
        }

        public override string ToString()
        {
            return $"{MonitorType}: {Address}";
        }

        public string GetNaturalKey()
        {
            return $"{MonitorType}|{Address.ToLowerInvariant()}";
        }
    }
}