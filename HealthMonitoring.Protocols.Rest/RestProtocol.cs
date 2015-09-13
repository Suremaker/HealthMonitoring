using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Protocols.Rest
{
    public class RestProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "rest"; } }
        public Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
