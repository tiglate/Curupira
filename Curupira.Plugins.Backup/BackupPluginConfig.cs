using System.Collections.Generic;

namespace Curupira.Plugins.Backup
{
    public class BackupPluginConfig
    {
        public string Destination { get; set; }

        public int Limit { get; set; }

        public IList<BackupArchive> Archives { get; private set; }

        public BackupPluginConfig()
        {
            Archives = new List<BackupArchive>();
        }
    }
}
