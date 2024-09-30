using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.Installer
{
    [ExcludeFromCodeCoverage]
    public class Component
    {
        public string Id { get; set; }
        public ComponentType Type { get; set; }
        public ComponentAction Action { get; set; }
        public IDictionary<string, string> Parameters { get; private set; }

        public IList<string> RemoveItems { get; private set; }

        public Component()
        {
            Parameters = new Dictionary<string, string>();
            RemoveItems = new List<string>();
        }
    }
}
