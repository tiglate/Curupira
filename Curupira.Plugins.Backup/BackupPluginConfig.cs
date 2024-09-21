using System.Collections.Generic;

namespace Curupira.Plugins.Backup
{
    public class BackupPluginConfig
    {
        public string Destination { get; set; }

        public int Limit { get; set; }

        public IList<BackupPackage> Packages { get; private set; }

        public BackupPluginConfig()
        {
            Packages = new List<BackupPackage>();
        }
    }
}
