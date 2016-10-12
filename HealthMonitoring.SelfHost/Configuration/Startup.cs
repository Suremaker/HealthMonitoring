using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using HealthMonitoring.Forwarders;
using HealthMonitoring.Hosting;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Persistence;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Handlers;
using HealthMonitoring.SelfHost.Security;
using HealthMonitoring.TaskManagement;
using HealthMonitoring.TimeManagement;
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

            ConfigureDependencies(config);
            ConfigureHandlers(config);
            ConfigureSerializers(config);
            ConfigureRoutes(config);
            ConfigureSwagger(config);
            config.EnableCors();
            appBuilder.UseWebApi(config);
        }

        private static void ConfigureHandlers(HttpConfiguration config)
        {
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());
            config.MessageHandlers.Add(new MessageLoggingHandler());
            
            var tokenProvider =
                config.DependencyResolver.GetService(typeof(ICredentialsProvider)) as ICredentialsProvider;
            var endpointRegistry = 
                config.DependencyResolver.GetService(typeof(IEndpointRegistry)) as IEndpointRegistry;
                
            config.Filters.Add(new AuthenticationFilter(endpointRegistry, tokenProvider));
        }

        private static void ConfigureSerializers(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            config.Formatters.Add(new TextMediaTypeFormatter());
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
                    c.CustomAsset("index", typeof(Startup).Assembly, "HealthMonitoring.SelfHost.Content.Swagger.swagger.html");
                });
        }

        private void ConfigureDependencies(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(Program).Assembly).Where(t => typeof(ApiController).IsAssignableFrom(t)).AsSelf();
            builder.RegisterAssemblyTypes(typeof(EndpointRegistry).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(CredentialsProvider).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(SqlEndpointConfigurationRepository).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterAssemblyTypes(typeof(EndpointStatsManager).Assembly).AsSelf().AsImplementedInterfaces().SingleInstance();
          
            builder.RegisterInstance<IEndpointMetricsForwarderCoordinator>(new EndpointMetricsForwarderCoordinator(PluginDiscovery<IEndpointMetricsForwarder>.DiscoverAllInCurrentFolder("*.Forwarders.*.dll")));
            builder.Register(ctx => ContinuousTaskExecutor<Endpoint>.StartExecutor(ctx.Resolve<ITimeCoordinator>())).AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TimeCoordinator>().AsImplementedInterfaces().SingleInstance();
            var container = builder.Build();

            InstantiateBackroundServices(container);
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void InstantiateBackroundServices(IContainer container)
        {
            container.Resolve<EndpointUpdateFrequencyGuard>();
            container.Resolve<EndpointStatsManager>();
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