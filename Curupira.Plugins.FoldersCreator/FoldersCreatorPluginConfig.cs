using System.Collections.Generic;

namespace Curupira.Plugins.FoldersCreator
{
    public class FoldersCreatorPluginConfig
    {
        public FoldersCreatorPluginConfig()
        {
             DirectoriesToCreate = new List<string>();
        }

        public IList<string> DirectoriesToCreate { get; private set; }
    }
}
