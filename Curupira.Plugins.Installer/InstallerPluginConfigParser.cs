using Curupira.Plugins.Contract;
using System;
using System.Xml;

namespace Curupira.Plugins.Installer
{
    public class InstallerPluginConfigParser : IPluginConfigParser<InstallerPluginConfig>
    {
        private readonly string _configFile;

        public InstallerPluginConfigParser(string configFile)
        {
            _configFile = configFile;
        }

        public InstallerPluginConfig Execute()
        {
            var configXml = new XmlDocument();
            configXml.Load(_configFile);
            return Execute(configXml.DocumentElement);
        }

        private static InstallerPluginConfig Execute(XmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                throw new ArgumentNullException(nameof(xmlConfig));
            }

            var pluginConfig = new InstallerPluginConfig();
            string namespaceUri = xmlConfig.NamespaceURI;

            XmlNodeList componentNodes = GetComponentNodes(xmlConfig, namespaceUri);
            ParseComponents(componentNodes, pluginConfig, namespaceUri);

            return pluginConfig;
        }

        private static XmlNodeList GetComponentNodes(XmlElement xmlConfig, string namespaceUri)
        {
            return xmlConfig.SelectNodes($"//*[local-name()='components']/*[local-name()='component' and namespace-uri()='{namespaceUri}']");
        }

        private static void ParseComponents(XmlNodeList componentNodes, InstallerPluginConfig pluginConfig, string namespaceUri)
        {
            if (componentNodes == null) return;

            foreach (XmlNode componentNode in componentNodes)
            {
                var component = CreateComponent(componentNode);
                ParseParams(componentNode, component, namespaceUri);
                ParseRemoveItemsIfZip(componentNode, component, namespaceUri);

                pluginConfig.Components.Add(component);
            }
        }

        private static Component CreateComponent(XmlNode componentNode)
        {
            string componentId = componentNode.Attributes["id"]?.Value;
            string typeStr = componentNode.Attributes["type"]?.Value;
            string actionStr = componentNode.Attributes["action"]?.Value;

            ValidateComponentAttributes(componentId, typeStr);

            ComponentType type = ParseComponentType(typeStr);
            ComponentAction? action = ParseActionIfMsi(type, actionStr);

            return new Component(id: componentId, type: type, action: action ?? ComponentAction.None);
        }

        private static void ValidateComponentAttributes(string componentId, string typeStr)
        {
            if (string.IsNullOrEmpty(componentId) || string.IsNullOrEmpty(typeStr))
            {
                throw new InvalidOperationException("Missing or empty 'id' or 'type' attribute in a component element.");
            }
        }

        private static ComponentType ParseComponentType(string typeStr)
        {
            if (!Enum.TryParse(typeStr, true, out ComponentType type))
            {
                throw new InvalidOperationException($"Invalid 'type' attribute value: {typeStr}. Valid values are: Zip, Msi, Bat, Exe");
            }

            return type;
        }

        private static ComponentAction? ParseActionIfMsi(ComponentType type, string actionStr)
        {
            if (type != ComponentType.Msi) return null;

            if (string.IsNullOrEmpty(actionStr))
            {
                throw new InvalidOperationException("Missing 'action' attribute for MSI component.");
            }

            if (!Enum.TryParse(actionStr, true, out ComponentAction parsedAction))
            {
                throw new InvalidOperationException($"Invalid 'action' attribute value: {actionStr}. Valid values for MSI are: Install, Uninstall");
            }

            return parsedAction;
        }

        private static void ParseParams(XmlNode componentNode, Component component, string namespaceUri)
        {
            foreach (XmlNode paramNode in componentNode.SelectNodes($"*[local-name()='param' and namespace-uri()='{namespaceUri}']"))
            {
                string paramName = paramNode.Attributes["name"]?.Value;
                string paramValue = paramNode.Attributes["value"]?.Value;

                if (string.IsNullOrEmpty(paramName))
                {
                    throw new InvalidOperationException("Missing or empty 'name' attribute in a param element.");
                }

                component.Parameters[paramName] = paramValue;
            }
        }

        private static void ParseRemoveItemsIfZip(XmlNode componentNode, Component component, string namespaceUri)
        {
            if (component.Type != ComponentType.Zip) return;

            foreach (XmlNode removeNode in componentNode.SelectNodes($"*[local-name()='remove' and namespace-uri()='{namespaceUri}']"))
            {
                component.RemoveItems.Add(removeNode.InnerText);
            }
        }
    }
}
