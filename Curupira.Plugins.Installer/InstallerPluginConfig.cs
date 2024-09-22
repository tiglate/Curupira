using System.Collections.Generic;

namespace Curupira.Plugins.Installer
{
    public class InstallerPluginConfig
    {
        public InstallerPluginConfig()
        {
            Components = new List<Component>();
        }

        public IList<Component> Components { get; private set; }
    }
}
