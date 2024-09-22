using System.Collections.Generic;

namespace Curupira.Plugins.ServiceManager
{
    public class ServiceManagerPluginConfig
    {
        public ServiceManagerPluginConfig()
        {
            Bundles = new Dictionary<string, Bundle>();
        }

        public IDictionary<string, Bundle> Bundles { get; private set; }
    }
}
