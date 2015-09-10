using System.Collections.Generic;
using Common.Logging;
using HealthMonitoring.Protocols;

namespace HealthMonitoring
{
    public class ProtocolRegistry : IProtocolRegistry
    {
        private static readonly ILog Logger = LogManager.GetLogger<ProtocolRegistry>();
        private readonly IDictionary<string, IHealthCheckProtocol> _protocols = new Dictionary<string, IHealthCheckProtocol>();

        public ProtocolRegistry(IEnumerable<IHealthCheckProtocol> protocols)
        {
            foreach (var protocol in protocols)
            {
                if (!_protocols.ContainsKey(protocol.Name))
                    _protocols.Add(protocol.Name, protocol);
                else
                    Logger.WarnFormat("Protocol with name {0} already exists. The {1} is not going to be registered", protocol.Name, protocol);
            }
        }

        public IEnumerable<IHealthCheckProtocol> Protocols { get { return _protocols.Values; } }
        public IHealthCheckProtocol FindByName(string protocolName)
        {
            IHealthCheckProtocol protocol;
            return _protocols.TryGetValue(protocolName, out protocol) ? protocol : null;
        }
    }
}