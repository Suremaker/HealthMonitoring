using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Model;
using HealthMonitoring.Monitors.Core.Exchange.Client.Entities;
using HealthMonitoring.TimeManagement;
using HealthMonitoring.Security;
using Newtonsoft.Json;

namespace HealthMonitoring.Monitors.Core.Exchange.Client
{
    public class HealthMonitorExchangeClient : IHealthMonitorExchangeClient
    {
        private readonly string _healthMonBaseUrl;
        private readonly ITimeCoordinator _timeCoordinator;
        private readonly ICredentialsProvider _credentialsProvider = new CredentialsProvider();

        public HealthMonitorExchangeClient(string healthMonBaseUrl, ITimeCoordinator timeCoordinator, ICredentialsProvider credentialsProvider)
        {
            if (string.IsNullOrWhiteSpace(healthMonBaseUrl))
                throw new ArgumentNullException(nameof(healthMonBaseUrl));

            _healthMonBaseUrl = healthMonBaseUrl;
            _timeCoordinator = timeCoordinator;
            _credentialsProvider = credentialsProvider;
        }

        public Task RegisterMonitorsAsync(IEnumerable<string> monitorTypes, CancellationToken token)
        {
            var credentials = _credentialsProvider.GetPullMonitorCredentials();

            return PostAsync(
                "/api/monitors/register", 
                monitorTypes.ToArray(),
                token,
                credentials
                );
        }

        public async Task<EndpointIdentity[]> GetEndpointIdentitiesAsync(CancellationToken token)
        {
            var result = await GetAsync("/api/endpoints/identities", token);
            return DeserializeJson<EndpointIdentity[]>(await result.Content.ReadAsStringAsync());
        }

        public Task UploadHealthAsync(EndpointHealthUpdate[] updates, CancellationToken token)
        {
        var credentials = _credentialsProvider.GetPullMonitorCredentials();

            return PostAsync(
                "/api/endpoints/health?clientCurrentTime=" + _timeCoordinator.UtcNow.ToString("u", CultureInfo.InvariantCulture), 
                updates.Select(u => new { EndpointId = u.EndpointId, Status = u.Health.Status, CheckTimeUtc = u.Health.CheckTimeUtc, ResponseTime = u.Health.ResponseTime, Details = u.Health.Details }), 
                token,
                credentials);
        }

        public async Task<HealthMonitorSettings> LoadSettingsAsync(CancellationToken token)
        {
            var result = await GetAsync("/api/config", token);
            var model = DeserializeJson<MonitorSettingsModel>(await result.Content.ReadAsStringAsync());
            return new HealthMonitorSettings(model.Monitor, new ThrottlingSettings(model.Throttling));
        }

        protected virtual HttpClient CreateClient()
        {
            return new HttpClient();
        }

        private HttpClient CreateAuthorizedClient(Credentials credentials)
        {
            var client = CreateClient();

            if (credentials == null)
                return client;

            var token = $"{credentials.MonitorId}:{credentials.PrivateToken}".ToBase64String();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            return client;
        }

        private async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T content, CancellationToken token, Credentials credentials = null)
        {
            using (var client = CreateAuthorizedClient(credentials))
            {
                var response = await client.PostAsync(_healthMonBaseUrl + endpoint, SerializeJson(content), token);
                return response.EnsureSuccessStatusCode();
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string endpoint, CancellationToken token, Credentials credentials = null)
        {
            using (var client = CreateAuthorizedClient(credentials))
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