using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Monitors.Http
{
    public class HttpMonitor : IHealthMonitor
    {
        private readonly string _name;

        public HttpMonitor() : this("http") { }

        protected HttpMonitor(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }

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
                    return new HealthInfo(HealthStatus.Healthy, sw.Elapsed, await ReadSuccessfulContent(response.Content));
                return new HealthInfo(HealthStatus.Faulty, sw.Elapsed, await GetFaultyResponseDetails(response));
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
