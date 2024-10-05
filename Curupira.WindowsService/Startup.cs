using Autofac.Integration.WebApi;
using System.Web.Http;
using Owin;
using Curupira.WindowsService.Infra.IoC;
using Autofac;
using Curupira.Plugins.Contract;
using Curupira.WindowsService.Attributes;

namespace Curupira.WindowsService
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var container = AutofacContainerBuilder.Configure();
            var logger = container.Resolve<ILogProvider>();

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

            config.Filters.Add(new GlobalExceptionFilterAttribute(logger));

            appBuilder.UseWebApi(config);

            logger.Info("Application started successfully.");
        }
    }
}
