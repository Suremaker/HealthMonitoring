using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace HealthMonitoring.SelfHost.Controllers
{
    public class StaticController : ApiController
    {
        [Route("dashboard/{file}")]
        public HttpResponseMessage GetStatic(string file)
        {
            return ReturnFileContent(file);
        }

        private static HttpResponseMessage ReturnFileContent(string file)
        {
            var path = string.Format("{0}content/{1}", AppDomain.CurrentDomain.BaseDirectory, file);

            if (!File.Exists(path))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(path),Encoding.UTF8,GetMediaType(file))
            };
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
                default:
                    return "text/plain";
            }
        }
    }
}