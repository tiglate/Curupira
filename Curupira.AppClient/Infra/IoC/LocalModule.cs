using Autofac;
using Curupira.AppClient.Services;
using NLog;

namespace Curupira.AppClient.Infra.IoC
{
    public class LocalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PluginExecutor>().As<IPluginExecutor>();
        }
    }
}