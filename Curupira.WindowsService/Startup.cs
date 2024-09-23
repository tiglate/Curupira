using Autofac.Integration.WebApi;
using System.Web.Http;
using Owin;
using NLog;
using Curupira.WindowsService.Infra;
using Curupira.WindowsService.Infra.IoC;

namespace Curupira.WindowsService
{
    public class Startup
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Configuration(IAppBuilder appBuilder)
        {
            var container = AutofacContainerBuilder.Configure();

            var config = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            // Force Web API to return JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new GlobalExceptionFilter(logger));

            appBuilder.UseWebApi(config);

            logger.Info("Application started successfully.");
        }
    }
}
