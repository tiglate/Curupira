using Autofac;
using Curupira.AppClient.Services;
using NLog;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.AppClient.Infra.IoC
{
    [ExcludeFromCodeCoverage]
    public class LocalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PluginExecutor>().As<IPluginExecutor>();
        }
    }
}