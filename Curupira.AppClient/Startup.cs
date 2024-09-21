using Autofac;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;
using Curupira.Plugins.ServiceManager;
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

            RegisterFoldersCreatorPlugin(builder);
            RegisterServiceManager(builder);
            RegisterBackupPlugin(builder);

            builder.RegisterType<PluginExecutor>().As<IPluginExecutor>();

            return builder.Build();
        }

        private static void RegisterBackupPlugin(ContainerBuilder builder)
        {
            builder.RegisterType<BackupPluginConfigParser>()
                .As<IPluginConfigParser<BackupPluginConfig>>()
                .WithParameter("configFile", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "backup-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<BackupPlugin>()
                .Named<IPlugin>("BackupPlugin");
        }

        private static void RegisterServiceManager(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceManagerPluginConfigParser>()
                .As<IPluginConfigParser<ServiceManagerPluginConfig>>()
                .WithParameter("configFile", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "service-manager-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<ServiceManagerPlugin>()
                .Named<IPlugin>("ServiceManagerPlugin");
        }

        private static void RegisterFoldersCreatorPlugin(ContainerBuilder builder)
        {
            builder.RegisterType<FoldersCreatorPluginConfigParser>()
                .As<IPluginConfigParser<FoldersCreatorPluginConfig>>()
                .WithParameter("configFile", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "folders-creator-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<FoldersCreatorPlugin>()
                .Named<IPlugin>("FoldersCreatorPlugin");
        }
    }
}
