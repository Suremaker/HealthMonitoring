using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace HealthMonitoring.AcceptanceTests.Helpers.Http
{
    internal static class MockWebEndpointExtensions
    {
        public static void SetupStatusResponse(this MockWebEndpoint endpoint, HttpStatusCode code)
        {
            endpoint.Reconfigure(builder => builder.WhenGet("/status").Respond(code));
        }

        public static void SetupStatusResponse(this MockWebEndpoint endpoint, HttpStatusCode code, object model)
        {
            endpoint.Reconfigure(builder => builder.WhenGet("/status").RespondContent(code, r => new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")));
        }

        public static void SetupStatusPlainResponse(this MockWebEndpoint endpoint, HttpStatusCode code, string text)
        {
            endpoint.Reconfigure(builder => builder.WhenGet("/status").RespondContent(code, r => new StringContent(text, Encoding.UTF8, "text/plain")));
        }
    }
}