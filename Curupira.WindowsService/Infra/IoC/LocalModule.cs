using Autofac;
using Autofac.Integration.WebApi;
using Curupira.WindowsService.Service;
using Curupira.WindowsService.Services;

namespace Curupira.WindowsService.Infra.IoC
{
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
