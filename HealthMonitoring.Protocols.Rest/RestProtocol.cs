using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace HealthMonitoring.Protocols.Rest
{
    public class RestProtocol : IHealthCheckProtocol
    {
        public string Name { get { return "rest"; } }

        public async Task<HealthInfo> CheckHealthAsync(string address, CancellationToken cancellationToken)
        {
            var client = new RestClient(address);
            var sw = Stopwatch.StartNew();
            var response = await client.ExecuteGetTaskAsync(new RestRequest(), cancellationToken);
            sw.Stop();
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new HealthInfo(HealthStatus.Inactive, sw.Elapsed);
            if (response.ErrorMessage != null)
                return new HealthInfo(HealthStatus.Faulty, sw.Elapsed, GetClientErrorDetails(response));
            if (response.StatusCode == HttpStatusCode.OK)
                return new HealthInfo(HealthStatus.Healthy, sw.Elapsed, JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content));
            return new HealthInfo(HealthStatus.Faulty, sw.Elapsed, GetFaultyResponseDetails(response));
        }

        private IReadOnlyDictionary<string, string> GetFaultyResponseDetails(IRestResponse response)
        {
            return new Dictionary<string, string>
            {
                {"code",response.StatusCode.ToString()},
                {"content",response.Content}
            };
        }

        private static Dictionary<string, string> GetClientErrorDetails(IRestResponse response)
        {
            return new Dictionary<string, string>
            {
                {"reason", response.ErrorMessage},
                {"exception", response.ErrorException != null ? response.ErrorException.ToString() : ""}
            };
        }
    }
}
