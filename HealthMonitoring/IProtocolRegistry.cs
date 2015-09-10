using System.Collections.Generic;
using HealthMonitoring.Protocols;

namespace HealthMonitoring
{
    public interface IProtocolRegistry
    {
        IEnumerable<IHealthCheckProtocol> Protocols { get; }
        IHealthCheckProtocol FindByName(string protocolName);
    }
}