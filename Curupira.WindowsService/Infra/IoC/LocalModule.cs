using Autofac;
using Autofac.Integration.WebApi;
using Curupira.WindowsService.Services;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Infra.IoC
{
    [ExcludeFromCodeCoverage]
    public class LocalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(System.Reflection.Assembly.GetExecutingAssembly());

            builder.RegisterType<PluginExecutorService>()
                .As<IPluginExecutorService>()
                .SingleInstance();

            builder.RegisterType<ServiceService>()
                .As<IServiceService>();

            builder.RegisterType<MyTaskService>()
                .As<IMyTaskService>();

            builder.RegisterType<EventLogService>()
                .As<IEventLogService>();

            builder.RegisterType<SystemUsageService>()
                .As<ISystemUsageService>();
        }
    }
}
