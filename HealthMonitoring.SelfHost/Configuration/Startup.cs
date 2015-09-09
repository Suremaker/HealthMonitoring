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
            ConfigureRoutes(config);
            ConfigureSwagger(config);
            appBuilder.UseWebApi(config);
        }

        private static void ConfigureRoutes(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("Api", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute("Swagger", "", null, null, new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));
        }

        private static void ConfigureSwagger(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Health Monitoring Service");
                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();
                    c.DescribeAllEnumsAsStrings();
                })
                .EnableSwaggerUi(c => { c.DisableValidator(); });
        }
    }
}