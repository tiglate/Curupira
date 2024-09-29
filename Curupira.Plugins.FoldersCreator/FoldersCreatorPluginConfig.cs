using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.FoldersCreator
{
    [ExcludeFromCodeCoverage]
    public class FoldersCreatorPluginConfig
    {
        public FoldersCreatorPluginConfig()
        {
             DirectoriesToCreate = new List<string>();
        }

        public IList<string> DirectoriesToCreate { get; private set; }
    }
}
