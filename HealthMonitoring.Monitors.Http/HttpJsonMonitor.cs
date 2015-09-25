using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HealthMonitoring.Monitors.Http
{
    public class HttpJsonMonitor : HttpMonitor
    {
        public HttpJsonMonitor() : base("http.json") { }

        protected override async Task<IReadOnlyDictionary<string, string>> ReadSuccessfulContent(HttpContent content)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(await content.ReadAsStringAsync());
        }
    }
}