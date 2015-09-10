using System.Configuration;
using System.Net;
using RestSharp;
using Xunit;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    public static class ClientHelper
    {
        public static RestClient Build()
        {
            return new RestClient(ConfigurationManager.AppSettings["BaseUrl"]);
        }

        public static IRestResponse ExpectAnSuccessfulGet(this RestClient client, string url)
        {
            var response = client.Get(new RestRequest(url));
            Assert.Null(response.ErrorException);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response;
        }
    }
}