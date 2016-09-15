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

        public static Guid RegisterEndpoint(this RestClient client, string monitor, string address, string @group, string name, string[] tags = null)
        {
            var response = client.Post(new RestRequest(EndpointRegistrationUrl).AddJsonBody(new { @group, monitorType = monitor, name, address, tags }));
            response.VerifyValidStatus(HttpStatusCode.Created);
            return JsonConvert.DeserializeObject<Guid>(response.Content);
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
    }
}