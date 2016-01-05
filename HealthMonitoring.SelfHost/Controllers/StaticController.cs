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
        private static readonly string InstanceTag = string.Format("\"{0}\"", Guid.NewGuid());

        [Route("dashboard")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetDashboard()
        {
            return ReturnFileContent("dashboard.html");
        }

        [Route("dashboard/details")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetEndpointDetails()
        {
            return ReturnFileContent("details.html");
        }

        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetHome()
        {
            return ReturnFileContent("home.html");
        }

        [Route("static/{file}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.NotModified)]
        public HttpResponseMessage GetStatic(string file)
        {
            return ReturnFileContent(file);
        }

        private HttpResponseMessage ReturnFileContent(string file)
        {
            if (Request.Headers.IfNoneMatch.Any(t => t.Tag == InstanceTag))
                return new HttpResponseMessage(HttpStatusCode.NotModified);

            var stream = GetCustomStream(file) ?? GetDefaultStream(file);
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

        protected virtual Stream GetCustomStream(string file)
        {
            var path = "content\\" + file;
            return File.Exists(path) ? File.OpenRead(path) : null;
        }

        private static Stream GetDefaultStream(string file)
        {
            var path = string.Format("HealthMonitoring.SelfHost.Content.{0}", file);
            var stream = typeof(StaticController).Assembly.GetManifestResourceStream(path);
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