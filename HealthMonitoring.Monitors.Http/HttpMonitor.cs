using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Http
{
    public class HttpMonitor : IHealthMonitor
    {
        static HttpMonitor()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
        }

        public HttpMonitor() : this("http") { }

        protected HttpMonitor(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(address, cancellationToken))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new HealthInfo(HealthStatus.NotExists);
                    case HttpStatusCode.ServiceUnavailable:
                        return new HealthInfo(HealthStatus.Offline);
                    case HttpStatusCode.OK:
                        return new HealthInfo(HealthStatus.Healthy, await ReadSuccessfulContent(response.Content));
                    default:
                        return new HealthInfo(HealthStatus.Faulty, await GetFaultyResponseDetails(response));
                }
            }
        }

        protected virtual Task<IReadOnlyDictionary<string, string>> ReadSuccessfulContent(HttpContent content)
        {
            IReadOnlyDictionary<string, string> result = new Dictionary<string, string>();
            return Task.FromResult(result);
        }

        private async Task<IReadOnlyDictionary<string, string>> GetFaultyResponseDetails(HttpResponseMessage response)
        {
            return new Dictionary<string, string>
            {
                {"code",response.StatusCode.ToString()},
                {"content",await response.Content.ReadAsStringAsync()}
            };
        }
    }
}
