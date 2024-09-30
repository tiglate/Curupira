using Autofac;
using Curupira.Plugins.IoC;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.AppClient.Infra.IoC
{
    [ExcludeFromCodeCoverage]
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
