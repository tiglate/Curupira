using Autofac;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;
using Curupira.Plugins.Installer;
using Curupira.Plugins.ServiceManager;
using System.IO;
using System;
using Curupira.Plugins.Common;
using System.Configuration;
using System.Linq;

namespace Curupira.Plugins.IoC
{
    public class CommonModule : Module
    {
        private readonly string _configDir;

        public CommonModule()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("ConfigDir"))
            {
                _configDir = ConfigurationManager.AppSettings["ConfigDir"];
            }

            if (string.IsNullOrWhiteSpace(_configDir))
            {
                _configDir = ConfigurationManager.AppSettings["ConfigDir"];
                _configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NLogProvider>()
                .As<ILogProvider>()
                .SingleInstance();

            RegisterFoldersCreatorPlugin(builder);
            RegisterServiceManager(builder);
            RegisterBackupPlugin(builder);
            RegisterInstallerPlugin(builder);
        }

        private void RegisterBackupPlugin(ContainerBuilder builder)
        {
            builder.RegisterType<BackupPluginConfigParser>()
                .As<IPluginConfigParser<BackupPluginConfig>>()
                .WithParameter("configFile", Path.Combine(_configDir, "backup-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<BackupPlugin>()
                .Named<IPlugin>("BackupPlugin");
        }

        private void RegisterInstallerPlugin(ContainerBuilder builder)
        {
            builder.RegisterType<InstallerPluginConfigParser>()
                .As<IPluginConfigParser<InstallerPluginConfig>>()
                .WithParameter("configFile", Path.Combine(_configDir, "Installer-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<InstallerPlugin>()
                .Named<IPlugin>("InstallerPlugin");
        }

        private void RegisterServiceManager(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceManagerPluginConfigParser>()
                .As<IPluginConfigParser<ServiceManagerPluginConfig>>()
                .WithParameter("configFile", Path.Combine(_configDir, "service-manager-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<ServiceManagerPlugin>()
                .Named<IPlugin>("ServiceManagerPlugin");
        }

        private void RegisterFoldersCreatorPlugin(ContainerBuilder builder)
        {
            builder.RegisterType<FoldersCreatorPluginConfigParser>()
                .As<IPluginConfigParser<FoldersCreatorPluginConfig>>()
                .WithParameter("configFile", Path.Combine(_configDir, "folders-creator-plugin.xml"))
                .SingleInstance();

            builder.RegisterType<FoldersCreatorPlugin>()
                .Named<IPlugin>("FoldersCreatorPlugin");
        }
    }
}
