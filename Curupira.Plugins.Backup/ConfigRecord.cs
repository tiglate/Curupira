namespace Curupira.Plugins.Backup
{
    internal class ConfigRecord
    {
        public bool IsFile { get; set; }

        public bool ContainsWildcard { get; set; }

        public string FileExtension { get; set; }

        public string Path { get; set; }

        public string Text { get; set; }
    }
}
