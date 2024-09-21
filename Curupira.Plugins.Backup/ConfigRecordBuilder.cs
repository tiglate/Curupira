using System.IO;

namespace Curupira.Plugins.Backup
{
    internal class ConfigRecordBuilder
    {
        private readonly string _rootDir;
        private readonly string _configText;

        public ConfigRecordBuilder(string rootDir, string configText)
        {
            if (configText.EndsWith("\\*")) // Something like "my_folder\sub_folder\*" is just a dir to be excluded...
            {
                configText = configText.Substring(0, configText.Length - 2);
            }
            _rootDir = rootDir;
            _configText = configText;
        }

        public ConfigRecord Build()
        {
            if (string.IsNullOrWhiteSpace(_configText))
            {
                return null;
            }

            var record = new ConfigRecord
            {
                Text = _configText
            };

            if (_configText.Contains("*."))
            {
                return ProcessWildcards(record) ? record : null;
            }

            return ProcessDirOrFile(record) ? record : null;
        }

        private bool ProcessDirOrFile(ConfigRecord record)
        {
            record.Path = Path.Combine(_rootDir, _configText).TrimEnd('\\', '/');

            var fileExits = File.Exists(record.Path);
            var dirExits = Directory.Exists(record.Path);

            if (!fileExits && !dirExits)
            {
                return false;
            }

            record.IsFile = fileExits;
            if (fileExits)
            {
                var startOfFileExtension = record.Path.IndexOf("*.");
                if (startOfFileExtension != -1)
                {
                    record.FileExtension = record.Path.Substring(startOfFileExtension + 1);
                }
            }
            return true;
        }

        private bool ProcessWildcards(ConfigRecord record)
        {
            record.IsFile = true;
            record.ContainsWildcard = true;

            var startOfFileExtension = _configText.IndexOf("*.");
            record.FileExtension = _configText.Substring(startOfFileExtension + 1);

            if (startOfFileExtension > 0)
            {
                var pathBeforeTheWildcard = _configText.Substring(0, startOfFileExtension);
                record.Path = Path.Combine(_rootDir, pathBeforeTheWildcard).TrimEnd('\\', '/');

                if (!Directory.Exists(record.Path))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
