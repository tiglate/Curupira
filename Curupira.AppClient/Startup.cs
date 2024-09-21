using Autofac;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;
using System.IO;
using System;

namespace Curupira.AppClient
{
    public static class Startup
    {
        public static IContainer ConfigureServices(Options options = null)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<NLogProvider>()
                .As<ILogProvider>()
                .SingleInstance();

            builder.RegisterType<FoldersCreatorPluginConfigParser>()
                .As<IPluginConfigParser<FoldersCreatorPluginConfig>>()
                .WithParameter("configFile", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "folders-creator-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<FoldersCreatorPlugin>()
                .Named<IPlugin>("FoldersCreatorPlugin");

            builder.RegisterType<BackupPluginConfigParser>()
                .As<IPluginConfigParser<BackupPluginConfig>>()
                .WithParameter("configFile", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "backup-plugin.xml"))
                .SingleInstance();
            
            builder.RegisterType<BackupPlugin>()
                .Named<IPlugin>("BackupPlugin");

            builder.RegisterType<PluginExecutor>().As<IPluginExecutor>();

            return builder.Build();
        }
    }
}
