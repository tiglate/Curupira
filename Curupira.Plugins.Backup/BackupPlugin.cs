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
        private volatile bool _killed = false;

        public BackupPlugin(ILogProvider logger, IPluginConfigParser<BackupPluginConfig> configParser)
            : base("Backup Plugin", logger, configParser)
        {
        }

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            _killed = false;
            try
            {
                foreach (var backupPackage in Config.Packages)
                {
                    if (_killed)
                    {
                        Logger.Info(FormatLogMessage(nameof(Execute), "Plugin execution cancelled."));
                        return false;
                    }

                    string zipFileName = $"{DateTime.Now:yyyyMMddhhmmss}-{backupPackage.Id}.zip";
                    string zipFilePath = Path.Combine(Config.Destination, zipFileName);

                    // Enforce backup limit if specified
                    EnforceBackupLimit(backupPackage.Id);

                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        // Add files and directories to the zip archive
                        AddItemsToZip(zipArchive, backupPackage);
                    }

                    Logger.Info($"Backup '{backupPackage.Id}' created successfully at '{zipFilePath}'.");
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

        private void AddItemsToZip(ZipArchive zipArchive, BackupPackage backupPackage)
        {
            // If there are no 'add' items, add the entire root directory (except for removed items)
            if (backupPackage.AddItems.Count == 0)
            {
                foreach (string itemPath in GetFilesToBeAddedToZip(backupPackage))
                {
                    AddItemToZip(zipArchive, itemPath, backupPackage.Root);
                }
            }
            else // If there are 'add' items, process them 
            {
                //foreach (string pattern in backupPackage.AddItems)
                //{
                //    string fullPathPattern = Path.Combine(backupPackage.Root, pattern);
                //    foreach (string itemPath in GetMatchingItems(fullPathPattern))
                //    {
                //        if (!backupPackage.RemoveItems.Any(removeItem =>
                //            MatchesPattern(itemPath, Path.Combine(backupPackage.Root, removeItem))))
                //        {
                //            AddItemToZip(zipArchive, itemPath, backupPackage.Root);
                //        }
                //    }
                //}
            }
        }

        private IList<string> GetFilesToBeAddedToZip(BackupPackage backupPackage)
        {
            var filesList = new List<string>();
            var matcher = new FileMatcher(backupPackage.Root, backupPackage.RemoveItems);
            GetFilesToBeAddedToZip(new DirectoryInfo(backupPackage.Root), filesList, matcher);
            return filesList;
        }

        private void GetFilesToBeAddedToZip(DirectoryInfo directory, List<string> filesList, FileMatcher matcher)
        {
            filesList.AddRange(
                directory
                    .GetFiles()
                    .Where(file => !matcher.IsMatch(file.FullName))
                    .Select(file => file.FullName));

            foreach (var subdirectory in directory.GetDirectories())
            {
                if (!matcher.IsMatch(subdirectory.FullName))
                {
                    GetFilesToBeAddedToZip(subdirectory, filesList, matcher);
                }
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
                zipArchive.CreateEntry(entryName + "/"); // Create a directory entry
                foreach (string filePath in Directory.EnumerateFiles(itemPath, "*", SearchOption.AllDirectories))
                {
                    AddItemToZip(zipArchive, filePath, rootPath);
                }
                Logger.Debug($"Added directory to zip: {entryName}");
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