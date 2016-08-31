using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange.Client.Entities;
using Newtonsoft.Json;

namespace HealthMonitoring.Monitors.Core.Exchange.Client
{
    public class HealthMonitorExchangeClient : IHealthMonitorExchangeClient
    {
        private readonly string _healthMonBaseUrl;

        public HealthMonitorExchangeClient(string healthMonBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(healthMonBaseUrl))
                throw new ArgumentNullException(nameof(healthMonBaseUrl));

            _healthMonBaseUrl = healthMonBaseUrl;
        }

        public Task RegisterMonitorsAsync(IEnumerable<string> monitorTypes, CancellationToken token)
        {
            return PostAsync("/api/monitors/register", monitorTypes.ToArray(), token);
        }

        public async Task<EndpointIdentity[]> GetEndpointIdentitiesAsync(CancellationToken token)
        {
            var result = await GetAsync("/api/endpoints/identities", token);
            return DeserializeJson<EndpointIdentity[]>(await result.Content.ReadAsStringAsync());
        }

        public Task UploadHealthAsync(EndpointHealthUpdate[] updates, CancellationToken token)
        {
            return PostAsync("/api/endpoints/health", updates.Select(u => new { EndpointId = u.EndpointId, Status = u.Health.Status, CheckTimeUtc = u.Health.CheckTimeUtc, ResponseTime = u.Health.ResponseTime, Details = u.Health.Details }), token);
        }

        public async Task<HealthMonitorSettings> LoadSettingsAsync(CancellationToken token)
        {
            var result = await GetAsync("/api/config", token);
            var model = DeserializeJson<MonitorSettingsModel>(await result.Content.ReadAsStringAsync());
            return new HealthMonitorSettings(model.Monitor, new ThrottlingSettings(model.Throttling));
        }

        private static HttpClient CreateClient()
        {
            return new HttpClient();
        }

        private async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T content, CancellationToken token)
        {
            using (var client = CreateClient())
            {
                var response = await client.PostAsync(_healthMonBaseUrl + endpoint, SerializeJson(content), token);
                return response.EnsureSuccessStatusCode();
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string endpoint, CancellationToken token)
        {
            using (var client = CreateClient())
            {
                var response = await client.GetAsync(_healthMonBaseUrl + endpoint, token);
                return response.EnsureSuccessStatusCode();
            }
        }

        private T DeserializeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static StringContent SerializeJson<T>(T content)
        {
            return new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
        }
    }
}