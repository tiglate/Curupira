﻿using Curupira.Plugins.Contract;
using System;
using System.Xml;

namespace Curupira.Plugins.Backup
{
    public class BackupPluginConfigParser : IPluginConfigParser<BackupPluginConfig>
    {
        public BackupPluginConfig Execute(XmlElement xmlConfig)
        {
            var pluginConfig = new BackupPluginConfig();

            if (xmlConfig == null)
                throw new ArgumentNullException(nameof(xmlConfig));

            // Get the namespace URI from the XML document
            string namespaceUri = xmlConfig.NamespaceURI;

            // Read settings
            XmlNode settingsNode = xmlConfig.SelectSingleNode("//*[local-name()='settings' and namespace-uri()='" + namespaceUri + "']");
            pluginConfig.Destination = settingsNode?.Attributes["destination"]?.Value;
            if (string.IsNullOrEmpty(pluginConfig.Destination))
            {
                throw new InvalidOperationException("Missing or empty 'destination' attribute in settings.");
            }

            string limitStr = settingsNode?.Attributes["limit"]?.Value;
            if (!string.IsNullOrEmpty(limitStr))
            {
                if (!int.TryParse(limitStr, out int localLimit) || localLimit <= 0)
                {
                    throw new InvalidOperationException("Invalid 'limit' attribute in settings. It must be a positive integer.");
                }
                pluginConfig.Limit = localLimit;
            }

            // Read backup packages
            XmlNodeList backupNodes = xmlConfig.SelectNodes("//*[local-name()='backups']/*[local-name()='backup' and namespace-uri()='" + namespaceUri + "']");
            if (backupNodes != null)
            {
                foreach (XmlNode backupNode in backupNodes)
                {
                    string id = backupNode.Attributes["id"]?.Value;
                    string root = backupNode.Attributes["root"]?.Value;
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(root))
                        throw new InvalidOperationException("Missing or empty 'id' or 'root' attribute in a backup element.");

                    var backupPackage = new BackupPackage(id, root);

                    foreach (XmlNode addNode in backupNode.SelectNodes("*[local-name()='add' and namespace-uri()='" + namespaceUri + "']"))
                    {
                        backupPackage.AddItems.Add(addNode.InnerText.TrimEnd('\\', '/'));
                    }

                    foreach (XmlNode removeNode in backupNode.SelectNodes("*[local-name()='remove' and namespace-uri()='" + namespaceUri + "']"))
                    {
                        backupPackage.RemoveItems.Add(removeNode.InnerText.TrimEnd('\\', '/'));
                    }

                    pluginConfig.Packages.Add(backupPackage);
                }
            }

            return pluginConfig;
        }
    }
}
