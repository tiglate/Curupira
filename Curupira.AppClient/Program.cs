using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;
using Curupira.Plugins.FoldersCreator;

namespace Curupira.AppClient
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            //var toBeRemoved = "\\*.bat";
            //var startOfFileExtension = toBeRemoved.IndexOf("*.");
            //var fileExtension = toBeRemoved.Substring(startOfFileExtension + 1);
            //var restOfThePath = toBeRemoved.Substring(0, startOfFileExtension);
            //Console.WriteLine(startOfFileExtension);
            //Console.WriteLine(fileExtension);
            //Console.WriteLine(restOfThePath);
            //Console.Write("Press any key to continue...");
            //Console.ReadKey();

            //return;
            TestFolderCreation().Wait();
            TestBackup().Wait();
            NLog.LogManager.Shutdown();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
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
            plugin.Init(ReadConfigFile(configFile));
            plugin.Progress += (sender, e) =>
            {
                Console.WriteLine($"Progress: {e.Percentage}% - {e.Message}");
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