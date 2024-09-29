using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.Backup
{
    [ExcludeFromCodeCoverage]
    public class BackupPluginConfig
    {
        public string Destination { get; set; }

        public int Limit { get; set; }

        public IList<BackupArchive> Archives { get; private set; }

        public BackupPluginConfig(string destination = null, int limit = 0)
        {
            Destination = destination;
            Limit = limit;
            Archives = new List<BackupArchive>();
        }
    }
}
