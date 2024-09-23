using Autofac;
using Curupira.Plugins.IoC;

namespace Curupira.AppClient.Infra.IoC
{
    public static class AutofacContainerBuilder
    {
        public static IContainer Configure(Options options = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<LocalModule>();
            return builder.Build();
        }
    }
}
