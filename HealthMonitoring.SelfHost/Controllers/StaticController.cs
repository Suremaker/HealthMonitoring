using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class StaticController : ApiController
    {
        [Route("dashboard")]
        public HttpResponseMessage GetDashboard()
        {
            return ReturnFileContent("dashboard.html");
        }

        [Route("dashboard/details")]
        public HttpResponseMessage GetEndpointDetails()
        {
            return ReturnFileContent("details.html");
        }

        [Route("")]
        public HttpResponseMessage GetHome()
        {
            return ReturnFileContent("home.html");
        }

        [Route("static/{file}")]
        public HttpResponseMessage GetStatic(string file)
        {
            return ReturnFileContent(file);
        }

        private static HttpResponseMessage ReturnFileContent(string file)
        {
            var stream = GetCustomStream(file)??GetDefaultStream(file);
            if (stream == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMediaType(file));
            return response;
        }

        private static Stream GetCustomStream(string file)
        {
            var path = "content\\" + file;
            return File.Exists(path) ? File.OpenRead(path) : null;
        }

        private static Stream GetDefaultStream(string file)
        {
            var path = string.Format("HealthMonitoring.SelfHost.Content.{0}", file);
            var stream = typeof (StaticController).Assembly.GetManifestResourceStream(path);
            return stream;
        }

        private static string GetMediaType(string file)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            switch (ext)
            {
                case ".html":
                    return "text/html";
                case ".js":
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".ico":
                    return "image/x-icon";
                default:
                    return "text/plain";
            }
        }
    }
}