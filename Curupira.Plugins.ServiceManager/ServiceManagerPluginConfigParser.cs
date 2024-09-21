using Curupira.Plugins.Contract;
using System;
using System.Collections.Generic;
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

            // Get the namespace URI from the XML document
            string namespaceUri = xmlConfig.NamespaceURI;

            // Read service bundles
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

                    var serviceActions = new List<ServiceAction>();

                    // Use 'service' instead of 'services'
                    foreach (XmlNode serviceNode in bundleNode.SelectNodes($"*[local-name()='service' and namespace-uri()='{namespaceUri}']"))
                    {
                        string serviceName = serviceNode.Attributes["name"]?.Value;
                        string actionStr = serviceNode.Attributes["action"]?.Value;

                        if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(actionStr))
                        {
                            throw new InvalidOperationException("Missing or empty 'name' or 'action' attribute in a service element.");
                        }

                        if (!Enum.TryParse(actionStr, true, out ActionEnum action))
                        {
                            throw new InvalidOperationException($"Invalid 'action' attribute value: {actionStr}. Valid values are: Start, Stop");
                        }

                        serviceActions.Add(new ServiceAction { ServiceName = serviceName, Action = action });
                    }

                    pluginConfig.Bundles.Add(bundleId, serviceActions);
                }
            }
            return pluginConfig;
        }
    }
}
