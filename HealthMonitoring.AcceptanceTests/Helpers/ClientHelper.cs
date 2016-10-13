using System;
using System.Configuration;
using System.Linq;
using System.Net;
using HealthMonitoring.AcceptanceTests.Helpers.Entities;
using Newtonsoft.Json;
using RestSharp;
using Xunit;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    static class ClientHelper
    {
        public const string EndpointRegistrationUrl = "/api/endpoints/register";
        public const string RegisterEndpointSecret = "PostRegisterEndpoint";
        public static readonly CredentialsProvider CredentialsProvider = new CredentialsProvider();

        public static RestClient Build()
        {
            var client = new RestClient(GetBaseUrl());

            Wait.Until(
                Timeouts.Default,
                () => client.Get(new RestRequest("/api/monitors")).StatusCode,
                status => status == HttpStatusCode.OK,
                "Service is not functioning");

            return client;
        }

        private static Uri GetBaseUrl()
        {
            return new Uri(ConfigurationManager.AppSettings["BaseUrl"]);
        }

        public static IRestResponse ExpectAnSuccessfulGet(this RestClient client, string url)
        {
            var response = client.Get(new RestRequest(url));
            VerifyValidStatus(response, HttpStatusCode.OK);
            return response;
        }

        public static void VerifyValidStatus(this IRestResponse response, HttpStatusCode status)
        {
            Assert.Null(response.ErrorException);
            Assert.Equal(status, response.StatusCode);
        }

        public static void VerifyHeader(this IRestResponse response, string header, string value)
        {
            var headerObject = response.Headers.SingleOrDefault(h => String.Equals(h.Name, header, StringComparison.OrdinalIgnoreCase));
            Assert.True(headerObject != null, $"Header {header} is missing");
            Assert.Equal(value, headerObject.Value);
        }

        public static void VerifyLocationHeader(this IRestResponse response, string url)
        {
            response.VerifyHeader("location", new Uri(GetBaseUrl(), url).ToString());
        }

        public static Guid RegisterEndpoint(this RestClient client, string monitor, string address, string group, string name, string[] tags = null, Credentials credentials = null)
        {
            var request = new RestRequest(EndpointRegistrationUrl)
                .AddJsonBody(new {group, monitorType = monitor, name, address, tags})
                .Authorize(credentials);
            var response = client.Post(request);
            response.VerifyValidStatus(HttpStatusCode.Created);
            return JsonConvert.DeserializeObject<Guid>(response.Content);
        }

        public static void EnsureStatusChanged(this RestClient client, Guid endpointId, EndpointStatus expectedStatus)
        {
            Wait.Until(Timeouts.Default,
                () => client.GetEndpointDetails(endpointId),
                e => e.Status == expectedStatus,
                "Endpoint status did not changed to " + expectedStatus);
        }

        public static void EnsureMonitoringStarted(this RestClient client, Guid endpointId)
        {
            Wait.Until(
                Timeouts.Default,
                () => client.GetEndpointDetails(endpointId),
                e => e.LastResponseTime.GetValueOrDefault() > TimeSpan.Zero,
                "Endpoint monitoring did not started");
        }

        public static EndpointEntity GetEndpointDetails(this RestClient client, Guid identifier)
        {
            return client.Get(new RestRequest("/api/endpoints/" + identifier)).DeserializeEndpointDetails();
        }

        public static EndpointEntity DeserializeEndpointDetails(this IRestResponse response)
        {
            response.VerifyValidStatus(HttpStatusCode.OK);
            return JsonConvert.DeserializeObject<EndpointEntity>(response.Content);
        }

        public static EndpointHealthStats[] DeserializeEndpointStats(this IRestResponse response)
        {
            response.VerifyValidStatus(HttpStatusCode.OK);
            return JsonConvert.DeserializeObject<EndpointHealthStats[]>(response.Content);
        }

        public static IRestRequest Authorize(this IRestRequest request, Credentials credentials)
        {
            if(credentials == null)
                return request;

            var authInfo = CredentialsProvider.EncryptBase64String($"{credentials.MonitorId}:{credentials.PrivateToken}");
            request.AddHeader("Authorization", $"Basic {authInfo}");
            return request;
        }
    }
}