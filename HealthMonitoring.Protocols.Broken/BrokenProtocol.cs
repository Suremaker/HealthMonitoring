using System;

namespace HealthMonitoring.Protocols.Broken
{
    public class BrokenProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "broken"; } }

        public BrokenProtocol()
        {
            throw new Exception("something is broken");
        }

    }
}
