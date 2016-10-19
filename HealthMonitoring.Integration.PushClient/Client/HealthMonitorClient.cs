using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using HealthMonitoring.Integration.PushClient.Client.Models;
using HealthMonitoring.Integration.PushClient.Registration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HealthMonitoring.Integration.PushClient.Client
{
    internal class HealthMonitorClient : IHealthMonitorClient
    {
        private const string PushMonitorType = "push";
        private static readonly ILog _logger = LogManager.GetLogger<HealthMonitorClient>();
        private static readonly JsonConverter[] JsonConverters = { new StringEnumConverter() };
        private readonly string _healthMonitorUrl;

        public HealthMonitorClient(string healthMonitorUrl)
        {
            _healthMonitorUrl = healthMonitorUrl;
        }

        public async Task<Guid> RegisterEndpointAsync(EndpointDefinition definition, CancellationToken cancellationToken)
        {
            _logger.Info($"Registering endpoint Group={definition.GroupName}, Name={definition.EndpointName} in Health Monitor: {_healthMonitorUrl}");
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await PostAsync(client, "/api/endpoints/register", new
                    {
                        Name = definition.EndpointName,
                        Group = definition.GroupName,
                        Address = definition.Address,
                        MonitorType = PushMonitorType,
                        Tags = definition.Tags,
                        Password = definition.AuthenticationToken
                    }, cancellationToken);

                    return await DeserializeJsonAsync<Guid>(response);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Endpoint registration failed.", e);
                throw;
            }
        }

        public async Task<TimeSpan> GetHealthCheckIntervalAsync(CancellationToken cancellationToken)
        {
            _logger.Debug("Retrieving health check interval...");
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(_healthMonitorUrl + "/api/config", cancellationToken);
                var healthCheckInterval = (await DeserializeJsonAsync<HealthMonitorConfigurationModel>(response)).Monitor.HealthCheckInterval;
                _logger.Info($"Retrieved health check interval: {healthCheckInterval}");
                return healthCheckInterval;
            }
        }

        public async Task SendHealthUpdateAsync(Guid endpointId, string authenticationToken, HealthUpdate update, CancellationToken cancellationToken)
        {
            _logger.Info($"Sending health update: {update.Status}");
            using (var client = new HttpClient())
            {
                Authorize(client, endpointId, authenticationToken);
                var result = await PostAsync(client, $"/api/endpoints/{endpointId}/health?clientCurrentTime={DateTimeOffset.UtcNow.ToString("u", CultureInfo.InvariantCulture)}", update, cancellationToken);
                if (result.StatusCode == HttpStatusCode.NotFound)
                    throw new EndpointNotFoundException();
                result.EnsureSuccessStatusCode();
            }
        }

        private void Authorize(HttpClient client, Guid endpointId, string authenticationToken)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{endpointId}:{authenticationToken}")));
        }

        private Task<HttpResponseMessage> PostAsync<T>(HttpClient client, string url, T request, CancellationToken cancellationToken)
        {
            return client.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, _healthMonitorUrl + url) { Content = SerializeJson(request) },
                cancellationToken);
        }

        private static async Task<T> DeserializeJsonAsync<T>(HttpResponseMessage message)
        {
            return JsonConvert.DeserializeObject<T>(await message.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
        }

        private static StringContent SerializeJson<T>(T content)
        {
            return new StringContent(JsonConvert.SerializeObject(content, JsonConverters), Encoding.UTF8, "application/json");
        }
    }
}
