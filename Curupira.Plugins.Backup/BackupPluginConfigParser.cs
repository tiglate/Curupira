using Curupira.Plugins.Contract;
using System;
using System.IO;
using System.Xml;

namespace Curupira.Plugins.Backup
{
    public class BackupPluginConfigParser : IPluginConfigParser<BackupPluginConfig>
    {
        private readonly string _configFile;

        public BackupPluginConfigParser(string configFile)
        {
            _configFile = configFile;
        }

        public BackupPluginConfig Execute()
        {
            var configXml = new XmlDocument();
            configXml.Load(_configFile);
            return Execute(configXml.DocumentElement);
        }

        private BackupPluginConfig Execute(XmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                throw new ArgumentNullException(nameof(xmlConfig));
            }

            var namespaceUri = xmlConfig.NamespaceURI;

            var settingsNode = xmlConfig.SelectSingleNode($"//*[local-name()='settings' and namespace-uri()='{namespaceUri}']");

            var pluginConfig = new BackupPluginConfig(
                destination: GetGlobalDestinationDir(settingsNode),
                limit: GetBackupsLimit(settingsNode)
            );

            ParseBackupPackages(xmlConfig, pluginConfig, namespaceUri);

            return pluginConfig;
        }

        private static string GetGlobalDestinationDir(XmlNode settingsNode)
        {
            var destination = settingsNode?.Attributes["destination"]?.Value;

            if (!string.IsNullOrEmpty(destination) && !Directory.Exists(destination))
            {
                throw new DirectoryNotFoundException($"Invalid directory: '{destination}'");
            }

            return destination;
        }

        private static int GetBackupsLimit(XmlNode settingsNode)
        {
            var limitStr = settingsNode?.Attributes["limit"]?.Value;
            
            if (string.IsNullOrEmpty(limitStr))
            {
                return 0;
            }

            if (!int.TryParse(limitStr, out int localLimit) || localLimit <= 0)
            {
                throw new InvalidOperationException("Invalid 'limit' attribute in settings. It must be a positive integer.");
            }

            return localLimit;
        }

        private static void ParseBackupPackages(XmlElement xmlConfig, BackupPluginConfig pluginConfig, string namespaceUri)
        {
            var backupNodes = xmlConfig.SelectNodes($"//*[local-name()='backups']/*[local-name()='backup' and namespace-uri()='{namespaceUri}']");

            if (backupNodes == null || backupNodes.Count == 0)
            {
                return;
            }

            foreach (XmlNode backupNode in backupNodes)
            {
                string id = backupNode.Attributes["id"]?.Value;
                string root = backupNode.Attributes["root"]?.Value;
                string destination = backupNode.Attributes["destination"]?.Value;

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(root))
                {
                    throw new InvalidOperationException("Missing or empty 'id' or 'root' attribute in a backup element.");
                }

                if (string.IsNullOrWhiteSpace(pluginConfig.Destination) && string.IsNullOrWhiteSpace(destination))
                {
                    throw new InvalidOperationException("You need to specify the destination directory either globally in <settings> or locally in <backup>.");
                }

                var archive = new BackupArchive(id, root, destination);

                foreach (XmlNode removeNode in backupNode.SelectNodes($"*[local-name()='remove' and namespace-uri()='{namespaceUri}']"))
                {
                    archive.Exclusions.Add(removeNode.InnerText);
                }

                pluginConfig.Archives.Add(archive);
            }
        }
    }
}
