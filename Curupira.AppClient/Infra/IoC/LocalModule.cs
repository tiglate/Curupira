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
            builder.RegisterType<ProgressBarService>()
                .As<IProgressBarService>();

            builder.RegisterType<ConsoleService>()
                .As<IConsoleService>();

            builder.RegisterType<PluginExecutor>().As<IPluginExecutor>();
        }
    }
}