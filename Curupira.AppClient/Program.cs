using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Curupira.Plugins.Common;
using Curupira.Plugins.FoldersCreator;

namespace Curupira.AppClient
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            //
        }

        static void TestFolderCreation()
        {
            try
            {
                // Load XML configuration
                string configFilePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), "folderscreatorplugin.xml");
                XmlDocument configXml = new XmlDocument();
                configXml.Load(configFilePath);
                XmlElement configElement = configXml.DocumentElement;

                // Instantiate the plugin
                var plugin = new FoldersCreatorPlugin(new NLogProvider());
                plugin.Init(configElement);

                // Subscribe to the Progress event (optional)
                plugin.Progress += (sender, e) =>
                {
                    Console.WriteLine($"Progress: {e.Percentage}% - {e.Message}");
                };

                // Execute the plugin asynchronously
                IAsyncResult asyncResult = plugin.BeginExecute(new Dictionary<string, string>(), null, null);

                // Wait for completion (you can also do other work here while waiting)
                bool success = plugin.EndExecute(asyncResult);

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
            finally
            {
                NLog.LogManager.Shutdown();
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}