using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Backup
{
    public class BackupPlugin : BasePlugin<BackupPluginConfig>
    {
        private volatile bool _killed;

        public BackupPlugin(ILogProvider logger, IPluginConfigParser<BackupPluginConfig> configParser)
            : base("BackupPlugin", logger, configParser)
        {
        }

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            _killed = false;
            try
            {
                foreach (var archive in Config.Archives)
                {
                    if (_killed)
                    {
                        Logger.Info(FormatLogMessage(nameof(Execute), "Plugin execution cancelled."));
                        return false;
                    }

                    string zipFileName = $"{DateTime.Now:yyyyMMddhhmmss}-{archive.Id}.zip";
                    string zipFilePath = Path.Combine(Config.Destination, zipFileName);

                    // Enforce backup limit if specified
                    EnforceBackupLimit(archive.Id);

                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        // Add files and directories to the zip archive
                        AddItemsToZip(zipArchive, archive);
                    }

                    Logger.Info($"Backup '{archive.Id}' created successfully at '{zipFilePath}'.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred during backup.");
                return false;
            }
        }

        public override bool Kill()
        {
            _killed = true;
            return true;
        }

        public override void Dispose()
        {
            // This plugin doesn't have any resources to dispose.
        }

        private void AddItemsToZip(ZipArchive zipArchive, BackupArchive backupArchive)
        {
            var toBeAddedList = GetFilesToBeAddedToZip(backupArchive);
            var totalEntries = toBeAddedList.Count;
            var processedEntries = 0;

            foreach (string itemPath in toBeAddedList)
            {
                if (_killed)
                {
                    break;
                }
                AddItemToZip(zipArchive, itemPath, backupArchive.Root);
                processedEntries++;
                int percentage = (int)((double)processedEntries / totalEntries * 100);
                OnProgress(new PluginProgressEventArgs(percentage, $"Archive: {backupArchive.Id}. Processed {processedEntries} of {totalEntries} directories"));
            }
        }

        private void AddItemToZip(ZipArchive zipArchive, string itemPath, string rootPath)
        {
            string entryName = itemPath.Substring(rootPath.Length + 1); // Relative path within the zip

            if (File.Exists(itemPath))
            {
                zipArchive.CreateEntryFromFile(itemPath, entryName);
                Logger.Debug($"Added file to zip: {entryName}");
            }
            else if (Directory.Exists(itemPath))
            {
                zipArchive.CreateEntry($"{entryName}/"); // Create a directory entry
                Logger.Debug($"Added directory to zip: {entryName}");
            }
        }

        private IList<string> GetFilesToBeAddedToZip(BackupArchive archive)
        {
            var filesList = new List<string>();
            var matcher = new FileMatcher(archive.Root, archive.Exclusions);
            GetFilesToBeAddedToZip(new DirectoryInfo(archive.Root), filesList, matcher);
            return filesList;
        }

        private void GetFilesToBeAddedToZip(DirectoryInfo directory, List<string> filesList, FileMatcher matcher)
        {
            if (_killed)
            {
                return;
            }
            filesList.AddRange(
                directory
                    .GetFiles()
                    .Where(file => !matcher.IsMatch(file.FullName))
                    .Select(file => file.FullName));

            foreach (var subdirectory in directory.GetDirectories())
            {
                if (!matcher.IsMatch(subdirectory.FullName))
                {
                    filesList.Add(subdirectory.FullName);
                    GetFilesToBeAddedToZip(subdirectory, filesList, matcher);
                }
            }
        }

        private void EnforceBackupLimit(string backupId)
        {
            if (Config.Limit > 0)
            {
                string[] existingBackups = Directory.GetFiles(Config.Destination, $"*-{backupId}.zip");
                if (existingBackups.Length >= Config.Limit)
                {
                    // Delete the oldest backup file
                    string oldestBackup = existingBackups.OrderBy(f => File.GetCreationTime(f)).FirstOrDefault();
                    if (oldestBackup != null)
                    {
                        File.Delete(oldestBackup);
                        Logger.Info($"Deleted oldest backup for '{backupId}': {oldestBackup}");
                    }
                }
            }
        }
    }
}