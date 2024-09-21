using Curupira.Plugins.Contract;
using System;
using System.Xml;

namespace Curupira.Plugins.FoldersCreator
{
    public class FoldersCreatorPluginConfigParser : IPluginConfigParser<FoldersCreatorPluginConfig>
    {
        private readonly string _configFile;

        public FoldersCreatorPluginConfigParser(string configFile)
        {
            _configFile = configFile;
        }

        public FoldersCreatorPluginConfig Execute()
        {
            var configXml = new XmlDocument();
            configXml.Load(_configFile);
            return Execute(configXml.DocumentElement);
        }

        private FoldersCreatorPluginConfig Execute(XmlElement xmlConfig)
        {
            var pluginConfig = new FoldersCreatorPluginConfig();

            if (xmlConfig == null)
                throw new ArgumentNullException(nameof(xmlConfig));

            // Get the namespace URI from the XML document
            string namespaceUri = xmlConfig.NamespaceURI;

            // Use the namespace URI directly in the XPath expression
            XmlNodeList directoryNodes = xmlConfig.SelectNodes($"//*[local-name()='add' and namespace-uri()='{namespaceUri}']");

            if (directoryNodes != null)
            {
                foreach (XmlNode directoryNode in directoryNodes)
                {
                    pluginConfig.DirectoriesToCreate.Add(SanitizeDiretoryNames(directoryNode.InnerText));
                }
            }

            return pluginConfig;
        }

        private string SanitizeDiretoryNames(string dir)
        {
            if (dir.StartsWith("/"))
            {
                throw new NotSupportedException($"Linux directories are not supported! Invalid dir {dir}");
            }
            if (dir.Contains("/")) //Why use it in Windows?
            {
                dir = dir.Replace('/', '\\');
            }
            return dir;
        }
    }
}
