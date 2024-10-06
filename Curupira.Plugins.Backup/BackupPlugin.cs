using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Backup
{
    public class BackupPlugin : BasePlugin<BackupPluginConfig>
    {
        public BackupPlugin(ILogProvider logger, IPluginConfigParser<BackupPluginConfig> configParser)
            : base("BackupPlugin", logger, configParser)
        {
        }

        public override async Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs, CancellationToken cancelationToken = default)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            var archives = GetBackupArchives(commandLineArgs);

            if (archives == null || !archives.Any())
            {
                return false;
            }

            var success = true;

            try
            {
                // Process each archive in parallel using Task.WhenAll
                var tasks = archives.Select(async archive =>
                {
                    cancelationToken.ThrowIfCancellationRequested();

                    var destination = !string.IsNullOrEmpty(archive.Destination) ? archive.Destination : Config.Destination;
                    var zipFileName = $"{DateTime.Now:yyyyMMddhhmmss}-{archive.Id}.zip";
                    var zipFilePath = Path.Combine(destination, zipFileName);

                    if (!Directory.Exists(destination))
                    {
                        Directory.CreateDirectory(destination);
                    }

                    // Enforce the backup limit asynchronously
                    await Task.Run(() => EnforceBackupLimit(archive.Id)).ConfigureAwait(false);

                    // Create the zip archive asynchronously
                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        if (!await Task.Run(() => AddItemsToZip(zipArchive, archive, cancelationToken)).ConfigureAwait(false))
                        {
                            return false;
                        }
                    }

                    Logger.Info($"Backup '{archive.Id}' created successfully at '{zipFilePath}'.");
                    return true;
                });

                // Wait for all tasks to complete
                success = Array.TrueForAll(await Task.WhenAll(tasks).ConfigureAwait(false), successful => successful);
            }
            catch (OperationCanceledException)
            {
                Logger.Info(FormatLogMessage(nameof(ExecuteAsync), "Plugin execution cancelled."));
                success = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred during backup.");
                success = false;
            }

            return success;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.TraceMethod(nameof(BackupPlugin), nameof(Dispose));
            }
            // This plugin doesn't have any resources to dispose.
        }

        private IEnumerable<BackupArchive> GetBackupArchives(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(GetBackupArchives), nameof(commandLineArgs), commandLineArgs);

            var archiveId = (commandLineArgs != null && commandLineArgs.ContainsKey("archive") ? commandLineArgs["archive"] : null)
                         ?? (commandLineArgs != null && commandLineArgs.ContainsKey("backup") ? commandLineArgs["backup"] : null);

            BackupArchive selectedArchive = null;

            if (!string.IsNullOrEmpty(archiveId))
            {
                selectedArchive = Config.Archives.FirstOrDefault(p => p.Id.Equals(archiveId, StringComparison.CurrentCultureIgnoreCase));

                if (selectedArchive == null)
                {
                    Logger.Fatal(FormatLogMessage(nameof(ExecuteAsync), $"Archive '{archiveId}' not found."));
                    return Enumerable.Empty<BackupArchive>();
                }
            }

            return selectedArchive != null ? new[] { selectedArchive } : Config.Archives;
        }

        private bool AddItemsToZip(ZipArchive zipArchive, BackupArchive backupArchive, CancellationToken cancelationToken)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(AddItemsToZip), nameof(zipArchive), zipArchive, nameof(backupArchive), backupArchive);

            var toBeAddedList = GetFilesToBeAddedToZip(backupArchive, cancelationToken);
            var totalEntries = toBeAddedList.Count;
            var processedEntries = 0;

            foreach (string itemPath in toBeAddedList)
            {
                cancelationToken.ThrowIfCancellationRequested();

                if (AddItemToZip(zipArchive, itemPath, backupArchive.Root))
                {
                    processedEntries++;
                    int percentage = (int)((double)processedEntries / totalEntries * 100);
                    OnProgress(new PluginProgressEventArgs(percentage, $"Archive: {backupArchive.Id}. Processed {processedEntries} of {totalEntries} directories"));
                }
                else
                {
                    OnProgress(new PluginProgressEventArgs(100, $"Archive: {backupArchive.Id}. Error."));
                    return false;
                }
            }

            return true;
        }

        public virtual bool AddItemToZip(ZipArchive zipArchive, string itemPath, string rootPath)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(AddItemToZip), nameof(zipArchive), zipArchive, nameof(itemPath), itemPath, nameof(rootPath), rootPath);

            string entryName = itemPath.Substring(rootPath.Length + 1); // Relative path within the zip

            try
            {
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
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error to add '{itemPath}' to the zip file.");
                return false;
            }

            return true;
        }

        private IList<string> GetFilesToBeAddedToZip(BackupArchive archive, CancellationToken cancelationToken)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(GetFilesToBeAddedToZip), nameof(archive), archive);

            var filesList = new List<string>();
            var matcher = new FileMatcher(archive.Root, archive.Exclusions, Logger);
            GetFilesToBeAddedToZip(new DirectoryInfo(archive.Root), filesList, matcher, cancelationToken);
            return filesList;
        }

        private void GetFilesToBeAddedToZip(DirectoryInfo directory, List<string> filesList, FileMatcher matcher, CancellationToken cancelationToken)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(GetFilesToBeAddedToZip), nameof(directory), directory, nameof(filesList), filesList, nameof(matcher), matcher);

            cancelationToken.ThrowIfCancellationRequested();

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
                    GetFilesToBeAddedToZip(subdirectory, filesList, matcher, cancelationToken);
                }
            }
        }

        private void EnforceBackupLimit(string backupId)
        {
            Logger.TraceMethod(nameof(BackupPlugin), nameof(EnforceBackupLimit), nameof(backupId), backupId);

            // Only enforce the limit if it is greater than 0
            if (Config.Limit > 0)
            {
                // Get all existing backups for the current backupId
                string[] existingBackups = Directory.GetFiles(Config.Destination, $"*-{backupId}.zip");

                // If there are more backups than (Limit - 1), delete the oldest files to respect the limit
                int excessFileCount = existingBackups.Length - (Config.Limit - 1);

                // Delete the excess files, i.e., enough files to keep only (Limit - 1) backups
                if (excessFileCount > 0)
                {
                    // Find the oldest files based on their creation time
                    var filesToDelete = existingBackups
                        .OrderBy(f => File.GetCreationTime(f)) // Order files by creation time (oldest first)
                        .Take(excessFileCount) // Select the number of excess files to delete
                        .ToList();

                    foreach (var file in filesToDelete)
                    {
                        File.Delete(file);
                        Logger.Info($"Deleted oldest backup for '{backupId}': {file}");
                    }
                }
            }
        }
    }
}
