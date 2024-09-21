using System.Collections.Generic;

namespace Curupira.Plugins.Backup
{
    public class BackupPackage
    {
        public string Id { get; }
        public string Root { get; }
        public List<string> AddItems { get; } = new List<string>();
        public List<string> RemoveItems { get; } = new List<string>();

        public BackupPackage(string id, string root)
        {
            Id = id;
            Root = root;
        }
    }
}
