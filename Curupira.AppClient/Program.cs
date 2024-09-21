using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;
using NLog;
using ShellProgressBar;

namespace Curupira.AppClient
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ProgressBar progressBar;

        static Program()
        {
            progressBar = new ProgressBar(10000, "Loading");
        }

        static void Main(string[] args)
        {
            ApplyLogLevel();
            //return;
            //TestFolderCreation().Wait();
            TestBackup().Wait();
            LogManager.Shutdown();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        private static void ApplyLogLevel()
        {
            string logLevelSetting = ConfigurationManager.AppSettings["LogLevel"];

            LogLevel logLevel;
            switch (logLevelSetting?.ToUpper())
            {
                case "TRACE":
                    logLevel = LogLevel.Trace;
                    break;
                case "DEBUG":
                    logLevel = LogLevel.Debug;
                    break;
                case "INFO":
                    logLevel = LogLevel.Info;
                    break;
                case "WARN":
                    logLevel = LogLevel.Warn;
                    break;
                case "ERROR":
                    logLevel = LogLevel.Error;
                    break;
                case "FATAL":
                    logLevel = LogLevel.Fatal;
                    break;
                default:
                    logLevel = LogLevel.Info; // Default to Info level if not specified or invalid
                    break;
            }

            var config = LogManager.Configuration;
            var consoleRule = config.FindRuleByName("consoleRule");
            consoleRule?.SetLoggingLevels(logLevel, LogLevel.Fatal);
        }

        private static async Task TestBackup()
        {
            await TestPlugin<BackupPlugin, BackupPluginConfig, BackupPluginConfigParser>("backup-plugin.xml");
        }

        private static async Task TestFolderCreation()
        {
            await TestPlugin<FoldersCreatorPlugin, FoldersCreatorPluginConfig, FoldersCreatorPluginConfigParser>("folders-creator-plugin.xml");
        }

        private static async Task TestPlugin<TPlugin, TPluginConfig, TPluginConfigParser>(string configFile, IDictionary<string, string> commandLineArgs = null)
            where TPlugin : IPlugin
            where TPluginConfig : class
            where TPluginConfigParser : IPluginConfigParser<TPluginConfig>, new()
        {
            if (commandLineArgs == null)
            {
                commandLineArgs = new Dictionary<string, string>();
            }

            try
            {
                var plugin = BuildPlugin<TPlugin, TPluginConfig, TPluginConfigParser>(configFile);
                bool success = await plugin.ExecuteAsync(commandLineArgs);

                if (success)
                {
                    Logger.Info("Plugin executed successfully!");
                }
                else
                {
                    Logger.Error("Plugin execution failed.");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "A fatal error occurred in the application.");
            }
        }

        private static TPlugin BuildPlugin<TPlugin, TPluginConfig, TPluginConfigParser>(string configFile)
            where TPlugin : IPlugin
            where TPluginConfig : class
            where TPluginConfigParser : IPluginConfigParser<TPluginConfig>, new()
        {
            var plugin = (TPlugin)Activator.CreateInstance(typeof(TPlugin), new NLogProvider(), new TPluginConfigParser());
            progressBar.Message = $"Loading plugin: {plugin.Name}";
            plugin.Init(ReadConfigFile(configFile));
            plugin.Progress += (sender, e) =>
            {
                progressBar.Message = e.Message;
                var progress = progressBar.AsProgress<float>();
                progress.Report(e.Percentage / 100f);
            };

            return plugin;
        }

        private static XmlElement ReadConfigFile(string configFile)
        {
            string configFilePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFile);
            XmlDocument configXml = new XmlDocument();
            configXml.Load(configFilePath);
            return configXml.DocumentElement;
        }
    }
}