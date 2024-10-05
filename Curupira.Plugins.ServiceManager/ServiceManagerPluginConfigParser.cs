using Curupira.Plugins.Contract;
using System;
using System.Xml;

namespace Curupira.Plugins.ServiceManager
{
    public class ServiceManagerPluginConfigParser : IPluginConfigParser<ServiceManagerPluginConfig>
    {
        private readonly string _configFile;

        public ServiceManagerPluginConfigParser(string configFile)
        {
            _configFile = configFile;
        }

        public ServiceManagerPluginConfig Execute()
        {
            var configXml = new XmlDocument();
            configXml.Load(_configFile);
            return Execute(configXml.DocumentElement);
        }

        private ServiceManagerPluginConfig Execute(XmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                throw new ArgumentNullException(nameof(xmlConfig));
            }

            var pluginConfig = new ServiceManagerPluginConfig();

            string namespaceUri = xmlConfig.NamespaceURI;

            XmlNodeList bundleNodes = xmlConfig.SelectNodes($"//*[local-name()='bundles']/*[local-name()='bundle' and namespace-uri()='{namespaceUri}']");

            if (bundleNodes != null)
            {
                foreach (XmlNode bundleNode in bundleNodes)
                {
                    string bundleId = bundleNode.Attributes["id"]?.Value;
                    if (string.IsNullOrEmpty(bundleId))
                    {
                        throw new InvalidOperationException("Missing or empty 'id' attribute in a bundle element.");
                    }

                    var bundle = new Bundle(bundleId)
                    {
                        LogFile = bundleNode.Attributes["logFile"]?.Value
                    };

                    foreach (XmlNode serviceNode in bundleNode.SelectNodes($"*[local-name()='service' and namespace-uri()='{namespaceUri}']"))
                    {
                        string serviceName = serviceNode.Attributes["name"]?.Value;
                        string actionStr = serviceNode.Attributes["action"]?.Value;

                        if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(actionStr))
                        {
                            throw new InvalidOperationException("Missing or empty 'name' or 'action' attribute in a service element.");
                        }

                        if (!Enum.TryParse(actionStr, true, out Action action))
                        {
                            throw new InvalidOperationException($"Invalid 'action' attribute value: {actionStr}. Valid values are: Start, Stop");
                        }

                        bundle.Services.Add(new ServiceAction(serviceName, action));
                    }

                    pluginConfig.Bundles.Add(bundleId, bundle);
                }
            }
            return pluginConfig;
        }
    }
}
