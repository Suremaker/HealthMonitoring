using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Protocols.Broken
{
    public class BrokenProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "broken"; } }
        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public BrokenProtocol()
        {
            throw new Exception("something is broken");
        }

    }
}
