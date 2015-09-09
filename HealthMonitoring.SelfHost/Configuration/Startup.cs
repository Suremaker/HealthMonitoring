using System.Web.Http;
using Owin;
using Swashbuckle.Application;

namespace HealthMonitoring.SelfHost.Configuration
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Api", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("Swagger", "", null, null, new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));

            SwaggerConfig.Register(config);
            appBuilder.UseWebApi(config);
        }
    }
}