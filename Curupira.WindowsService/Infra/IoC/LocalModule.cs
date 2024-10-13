using Autofac;
using Autofac.Integration.WebApi;
using Curupira.WindowsService.Services;
using Curupira.WindowsService.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Infra.IoC
{
    [ExcludeFromCodeCoverage]
    public class LocalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(System.Reflection.Assembly.GetExecutingAssembly());

            RegisterWrappers(builder);

            RegisterServices(builder);
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<PluginExecutorService>()
                .As<IPluginExecutorService>()
                .SingleInstance();

            builder.RegisterType<WindowsServicesService>()
                .As<IWindowsServicesService>();

            builder.RegisterType<WindowsTasksService>()
                .As<IWindowsTasksService>();

            builder.RegisterType<EventLogService>()
                .As<IEventLogService>();

            builder.RegisterType<SystemUsageService>()
                .As<ISystemUsageService>();

            builder
                .RegisterType<FileUploadService>()
                .As<IFileUploadService>();
        }

        private static void RegisterWrappers(ContainerBuilder builder)
        {
            builder.RegisterType<TaskServiceWrapper>()
                .As<ITaskServiceWrapper>()
                .SingleInstance();

            builder.RegisterType<EventLogWrapperFactory>()
                .As<IEventLogWrapperFactory>();
        }
    }
}
