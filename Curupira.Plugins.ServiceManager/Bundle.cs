using System.Collections.Generic;

namespace Curupira.Plugins.ServiceManager
{
    public class Bundle
    {
        public string Id { get; set; }

        public string LogFile { get; set; }

        public IList<ServiceAction> Services { get; private set; }

        public Bundle(string id)
        {
            Id = id;
            Services = new List<ServiceAction>();
        }
    }
}
