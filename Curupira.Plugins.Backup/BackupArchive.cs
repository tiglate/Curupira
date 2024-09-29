using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.Backup
{
    [ExcludeFromCodeCoverage]
    public class BackupArchive
    {
        public string Id { get; }

        public string Root { get; }

        public string Destination { get; set; }

        public List<string> Exclusions { get; } = new List<string>();

        public BackupArchive(string id, string root, string destination = null)
        {
            Id = id;
            Root = root;
            Destination = destination;
        }
    }
}
