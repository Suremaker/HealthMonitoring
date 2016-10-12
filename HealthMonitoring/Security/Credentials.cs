
using System;

namespace HealthMonitoring.Security
{
    public class Credentials
    {
        public Credentials(Guid monitorId, string privateToken)
        {
            MonitorId = monitorId;
            PrivateToken = privateToken;
        }

        public override bool Equals(object obj)
        {
            Credentials cred = obj as Credentials;

            return !ReferenceEquals(null, cred)
                   && string.Equals(PrivateToken, cred.PrivateToken)
                   && MonitorId == cred.MonitorId;
        }

        public override int GetHashCode()
        {
            return PrivateToken.GetHashCode() ^ MonitorId.GetHashCode();
        }

        public string PrivateToken { get; }
        public Guid MonitorId { get; }
    }
}
