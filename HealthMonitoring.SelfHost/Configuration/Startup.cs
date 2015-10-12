using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using HealthMonitoring.Persistence;
using Microsoft.Owin.Host.HttpListener;
using Newtonsoft.Json.Converters;
using Owin;
using Swashbuckle.Application;

namespace HealthMonitoring.SelfHost.Configuration
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            ConfigureSerializers(config);
            ConfigureRoutes(config);
            ConfigureSwagger(config);
            ConfigureDependencies(config);
            appBuilder.UseWebApi(config);
        }

        private static void ConfigureSerializers(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }

        private static void ConfigureRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("Swagger", "api", null, null, new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));
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
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator();
                    c.CustomAsset("index", typeof(Startup).Assembly, "HealthMonitoring.SelfHost.Embedded.index.html");

                });
        }

        private void ConfigureDependencies(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(typeof(Program).Assembly).Where(t => typeof(ApiController).IsAssignableFrom(t)).AsSelf();
            builder.RegisterAssemblyTypes(typeof(HealthMonitorRegistry).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(SqlEndpointConfigurationStore).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();

            builder.RegisterInstance<IHealthMonitorRegistry>(new HealthMonitorRegistry(MonitorDiscovery.DiscoverAllInCurrentFolder()));
            var container = builder.Build();
            container.Resolve<HealthMonitor>();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private Assembly[] GetIndirectDependencies()
        {
            return new[]
            {
                typeof (OwinHttpListener).Assembly
            };
        }
    }
}