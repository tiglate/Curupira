using System.Collections.Generic;

namespace Curupira.Plugins.Backup
{
    public class BackupArchive
    {
        public string Id { get; }
        public string Root { get; }
        public List<string> Exclusions { get; } = new List<string>();

        public BackupArchive(string id, string root)
        {
            Id = id;
            Root = root;
        }
    }
}
