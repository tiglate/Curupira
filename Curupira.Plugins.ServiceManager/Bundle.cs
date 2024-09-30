using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
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
