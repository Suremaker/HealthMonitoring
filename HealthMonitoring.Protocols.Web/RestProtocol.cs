using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Protocols.Web
{
    public class WebProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "web"; } }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(address, cancellationToken))
            {
                sw.Stop();
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new HealthInfo(HealthStatus.Inactive, sw.Elapsed);
                if (response.StatusCode == HttpStatusCode.OK)
                    return new HealthInfo(HealthStatus.Healthy, sw.Elapsed);
                return new HealthInfo(HealthStatus.Faulty, sw.Elapsed, GetFaultyResponseDetails(response));
            }
        }

        private IReadOnlyDictionary<string, string> GetFaultyResponseDetails(HttpResponseMessage response)
        {
            return new Dictionary<string, string>
            {
                {"code",response.StatusCode.ToString()}
            };
        }
    }
}
