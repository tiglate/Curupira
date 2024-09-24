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
            return ParseConfig(configXml.DocumentElement);
        }

        private InstallerPluginConfig ParseConfig(XmlElement xmlConfig)
        {
            if (xmlConfig == null)
            {
                throw new ArgumentNullException(nameof(xmlConfig));
            }

            var pluginConfig = new InstallerPluginConfig();

            string namespaceUri = xmlConfig.NamespaceURI;

            XmlNodeList componentNodes = xmlConfig.SelectNodes($"//*[local-name()='components']/*[local-name()='component' and namespace-uri()='{namespaceUri}']");

            if (componentNodes != null)
            {
                foreach (XmlNode componentNode in componentNodes)
                {
                    string componentId = componentNode.Attributes["id"]?.Value;
                    string typeStr = componentNode.Attributes["type"]?.Value;
                    string actionStr = componentNode.Attributes["action"]?.Value;

                    if (string.IsNullOrEmpty(componentId) || string.IsNullOrEmpty(typeStr))
                    {
                        throw new InvalidOperationException("Missing or empty 'id' or 'type' attribute in a component element.");
                    }

                    if (!Enum.TryParse(typeStr, true, out ComponentType type))
                    {
                        throw new InvalidOperationException($"Invalid 'type' attribute value: {typeStr}. Valid values are: Zip, Msi, Bat, Exe");
                    }

                    // Only parse 'action' if it's an MSI component
                    ComponentAction? action = null;
                    if (type == ComponentType.Msi)
                    {
                        if (string.IsNullOrEmpty(actionStr))
                        {
                            throw new InvalidOperationException("Missing 'action' attribute for MSI component.");
                        }

                        if (!Enum.TryParse(actionStr, true, out ComponentAction parsedAction))
                        {
                            throw new InvalidOperationException($"Invalid 'action' attribute value: {actionStr}. Valid values for MSI are: Install, Uninstall");
                        }

                        action = parsedAction;
                    }

                    var component = new Component
                    {
                        Id = componentId,
                        Type = type,
                        Action = action ?? ComponentAction.None
                    };

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

                    if (type == ComponentType.Zip)
                    {
                        foreach (XmlNode removeNode in componentNode.SelectNodes($"*[local-name()='remove' and namespace-uri()='{namespaceUri}']"))
                        {
                            component.RemoveItems.Add(removeNode.InnerText);
                        }
                    }

                    pluginConfig.Components.Add(component);
                }
            }

            return pluginConfig;
        }
    }
}