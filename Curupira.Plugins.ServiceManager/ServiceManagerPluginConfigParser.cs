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

        private static ServiceManagerPluginConfig Execute(XmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                throw new ArgumentNullException(nameof(xmlConfig));
            }

            var pluginConfig = new ServiceManagerPluginConfig();
            string namespaceUri = xmlConfig.NamespaceURI;

            XmlNodeList bundleNodes = xmlConfig.SelectNodes($"//*[local-name()='bundles']/*[local-name()='bundle' and namespace-uri()='{namespaceUri}']");
            ParseBundles(bundleNodes, pluginConfig, namespaceUri);

            return pluginConfig;
        }

        private static void ParseBundles(XmlNodeList bundleNodes, ServiceManagerPluginConfig pluginConfig, string namespaceUri)
        {
            if (bundleNodes == null) return;

            foreach (XmlNode bundleNode in bundleNodes)
            {
                var bundle = CreateBundle(bundleNode);
                ParseServices(bundleNode, bundle, namespaceUri);
                pluginConfig.Bundles.Add(bundle.Id, bundle);
            }
        }

        private static Bundle CreateBundle(XmlNode bundleNode)
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

            return bundle;
        }

        private static void ParseServices(XmlNode bundleNode, Bundle bundle, string namespaceUri)
        {
            foreach (XmlNode serviceNode in bundleNode.SelectNodes($"*[local-name()='service' and namespace-uri()='{namespaceUri}']"))
            {
                var service = CreateServiceAction(serviceNode);
                bundle.Services.Add(service);
            }
        }

        private static ServiceAction CreateServiceAction(XmlNode serviceNode)
        {
            string serviceName = serviceNode.Attributes["name"]?.Value;
            string actionStr = serviceNode.Attributes["action"]?.Value;

            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(actionStr))
            {
                throw new InvalidOperationException("Missing or empty 'name' or 'action' attribute in a service element.");
            }

            if (!Enum.TryParse(actionStr, true, out Action action))
            {
                throw new InvalidOperationException($"Invalid 'action' attribute value: {actionStr}. Valid values are: Start, Stop.");
            }

            return new ServiceAction(serviceName, action);
        }
    }
}