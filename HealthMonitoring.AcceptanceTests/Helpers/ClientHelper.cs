using System;
using System.Configuration;
using System.Linq;
using System.Net;
using RestSharp;
using Xunit;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public static class ClientHelper
    {
        public static RestClient Build()
        {
            return new RestClient(GetBaseUrl());
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
            var headerObject = response.Headers.SingleOrDefault(h => string.Equals(h.Name, header, StringComparison.OrdinalIgnoreCase));
            Assert.True(headerObject != null, string.Format("Header {0} is missing", header));
            Assert.Equal(value, headerObject.Value);
        }

        public static void VerifyLocationHeader(this IRestResponse response, string url)
        {
            response.VerifyHeader("location", new Uri(GetBaseUrl(), url).ToString());
        }
    }
}