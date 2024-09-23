using Autofac;
using Curupira.Plugins.IoC;

namespace Curupira.WindowsService.Infra.IoC
{
    public static class AutofacContainerBuilder
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<LocalModule>();
            return builder.Build();
        }
    }
}
