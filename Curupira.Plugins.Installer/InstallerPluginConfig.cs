using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.Installer
{
    [ExcludeFromCodeCoverage]
    public class InstallerPluginConfig
    {
        public InstallerPluginConfig()
        {
            Components = new List<Component>();
        }

        public IList<Component> Components { get; private set; }
    }
}
