using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
    public class ServiceManagerPluginConfig
    {
        public ServiceManagerPluginConfig()
        {
            Bundles = new Dictionary<string, Bundle>();
        }

        public IDictionary<string, Bundle> Bundles { get; private set; }
    }
}
