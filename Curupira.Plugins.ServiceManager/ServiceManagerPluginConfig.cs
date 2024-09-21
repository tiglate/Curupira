using System.Collections.Generic;

namespace Curupira.Plugins.ServiceManager
{
    public class ServiceManagerPluginConfig
    {
        public ServiceManagerPluginConfig()
        {
            Bundles = new Dictionary<string, IList<ServiceAction>>();
        }

        public IDictionary<string, IList<ServiceAction>> Bundles { get; private set; }
    }
}
