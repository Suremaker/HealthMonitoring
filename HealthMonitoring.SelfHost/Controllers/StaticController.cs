using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class StaticController : ApiController
    {
        private static readonly string InstanceTag = $"\"{Guid.NewGuid()}\"";

        [Route("dashboard")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetDashboard()
        {
            return ReturnFileContent("Dashboard", "dashboard.html");
        }

        [Route("dashboard/details")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetEndpointDetails()
        {
            return ReturnFileContent("Details", "details.html");
        }

        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetHome()
        {
            return ReturnFileContent("Home", "home.html");
        }

        [Route("static/{directory}/{file}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetStatic(string directory, string file)
        {
            return ReturnFileContent(directory, file);
        }

        private HttpResponseMessage ReturnFileContent(string directory, string file)
        {
            if (Request.Headers.IfNoneMatch.Any(t => t.Tag == InstanceTag))
                return new HttpResponseMessage(HttpStatusCode.NotModified);

            var stream = GetCustomStream(directory, file) ?? GetDefaultStream(directory, file);
            if (stream == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };
            var extension = Path.GetExtension(file).ToLowerInvariant();
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMediaType(extension));
            response.Headers.ETag = new EntityTagHeaderValue(InstanceTag);
            return response;
        }

        protected virtual Stream GetCustomStream(string directory, string file)
        {
            var path = $"content\\{directory}\\{file}";
            return File.Exists(path) ? File.OpenRead(path) : null;
        }

        private static Stream GetDefaultStream(string directory, string file)
        {
            var path = $"HealthMonitoring.SelfHost.Content.{directory}.{file}";
            var assembly = typeof(StaticController).Assembly;
            var assemblyPath = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Equals(path, StringComparison.OrdinalIgnoreCase)) ?? path;
            var stream = assembly.GetManifestResourceStream(assemblyPath);
            return stream;
        }

        private static string GetMediaType(string extension)
        {
            switch (extension)
            {
                case ".html":
                    return "text/html";
                case ".js":
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".ico":
                    return "image/x-icon";
                case ".svg":
                    return "image/svg+xml";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                default:
                    return "text/plain";
            }
        }
    }
}