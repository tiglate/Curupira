using Curupira.Plugins.Contract;
using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace Curupira.Plugins.Installer
{
    public class ZipComponentHandler : IComponentHandler
    {
        private readonly ILogProvider _logger;

        public ZipComponentHandler(ILogProvider logger)
        {
            _logger = logger;
        }

        public event EventHandler<PluginProgressEventArgs> Progress;

        public Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(HandleAsync), nameof(component), component, nameof(ignoreUnauthorizedAccess), ignoreUnauthorizedAccess);

            var sourceFile = component.Parameters["SourceFile"];
            var targetDir = component.Parameters["TargetDir"];

            ValidateZipParameters(sourceFile, targetDir);

            _logger.Info($"Extracting '{sourceFile}' to '{targetDir}'...");

            using (var archive = ZipFile.OpenRead(sourceFile))
            {
                var processedEntries = 0;
                var totalEntries = archive.Entries.Count;

                foreach (var entry in archive.Entries)
                {
                    token.ThrowIfCancellationRequested();

                    if (ShouldSkipEntry(component, entry))
                    {
                        _logger.Debug($"Skipping removed entry: {entry.FullName}");
                        continue;
                    }

                    ProcessEntry(entry, targetDir, ignoreUnauthorizedAccess);

                    ReportProgress(++processedEntries, totalEntries);
                }
            }

            _logger.Info("Extraction completed successfully.");
            return Task.FromResult(true);
        }

        private void ValidateZipParameters(string sourceFile, string targetDir)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(ValidateZipParameters), nameof(sourceFile), sourceFile, nameof(targetDir), targetDir);

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(targetDir))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' or 'TargetDir' parameter for zip component.");
            }
        }

        private bool ShouldSkipEntry(Component component, ZipArchiveEntry entry)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(ShouldSkipEntry), nameof(component), component, nameof(entry), entry);

            return component.RemoveItems.Any(removeItem => MatchesPattern(entry.FullName.Replace("/", "\\"), removeItem));
        }

        private void ProcessEntry(ZipArchiveEntry entry, string targetDir, bool ignoreUnauthorizedAccess)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(ProcessEntry), nameof(entry), entry, nameof(targetDir), targetDir, nameof(ignoreUnauthorizedAccess), ignoreUnauthorizedAccess);

            var destinationPath = Path.Combine(targetDir, entry.FullName);

            if (IsDirectory(entry))
            {
                var canonicalDestinationPath = Path.GetFullPath(destinationPath);

                if (canonicalDestinationPath.StartsWith(targetDir, StringComparison.Ordinal))
                {
                    Directory.CreateDirectory(destinationPath);
                }
            }
            else
            {
                EnsureDirectoryExists(Path.GetDirectoryName(destinationPath));

                if (ignoreUnauthorizedAccess)
                {
                    TryExtractFile(entry, destinationPath);
                }
                else
                {
                    ExtractFile(entry, destinationPath, true);
                }
            }
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(EnsureDirectoryExists), nameof(directoryPath), directoryPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void TryExtractFile(ZipArchiveEntry entry, string destinationPath)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(TryExtractFile), nameof(entry), entry, nameof(destinationPath), destinationPath);

            try
            {
                ExtractFile(entry, destinationPath, true); // Overwrite existing files
            }
            catch (UnauthorizedAccessException)
            {
                _logger.Warn($"Impossible to create/override the file: '{destinationPath}'");
            }
        }

        private void ReportProgress(int processedEntries, int totalEntries)
        {
            int percentage = (int)((double)processedEntries / totalEntries * 100);
            OnProgress(new PluginProgressEventArgs(percentage, $"Extracted {processedEntries} of {totalEntries} entries"));
        }

        private bool MatchesPattern(string path, string pattern)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(MatchesPattern), nameof(path), path, nameof(pattern), pattern);

            // Escape special characters in the pattern
            string escapedPattern = Regex.Escape(pattern);

            // Replace wildcard characters with their regular expression equivalents
            string regexPattern = "^" + escapedPattern
                                        .Replace("\\*", ".*") // * matches zero or more characters
                                        .Replace("\\?", ".")  // ? matches any single character
                                 + "$";

            // Perform the regular expression match (case-insensitive)
            return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase);
        }

        protected virtual void ExtractFile(ZipArchiveEntry entry, string destinationPath, bool overwrite)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(ExtractFile), nameof(entry), entry, nameof(destinationPath), destinationPath, nameof(overwrite), overwrite);

            entry.ExtractToFile(destinationPath, overwrite);
        }

        private bool IsDirectory(ZipArchiveEntry entry)
        {
            _logger.TraceMethod(nameof(ZipComponentHandler), nameof(IsDirectory), nameof(entry), entry);

            if (entry == null)
            {
                return false;
            }
            return entry.FullName.Length > 0 && (entry.FullName[entry.FullName.Length - 1] == '/' || entry.FullName[entry.FullName.Length - 1] == '\\');
        }

        private void OnProgress(PluginProgressEventArgs e)
        {
            _logger.Debug($"{nameof(ZipComponentHandler)}: Progress: {e.Percentage}% - {e.Message}");
            Progress?.Invoke(this, e);
        }
    }
}
