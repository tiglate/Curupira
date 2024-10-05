using Curupira.Plugins.Contract;
using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Configuration;

namespace Curupira.Plugins.Installer
{
    public class ZipComponentHandler : BaseComponentHandler
    {
        private readonly long _maxAllowedUncompressedSize;
        private long totalExtractedSize;

        public ZipComponentHandler(ILogProvider logger)
            : base(logger)
        {
            var maxAllowedUncompressedSizeString = ConfigurationManager.AppSettings["MaxAllowedUncompressedSize"];

            if (!long.TryParse(maxAllowedUncompressedSizeString, out _maxAllowedUncompressedSize))
            {
                _maxAllowedUncompressedSize = 10737418240; // 10 Gb max allowed uncompressed size
            }
        }

        public override Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(HandleAsync), nameof(component), component, nameof(ignoreUnauthorizedAccess), ignoreUnauthorizedAccess);

            var sourceFile = component.Parameters["SourceFile"];
            var targetDir = component.Parameters["TargetDir"];

            ValidateZipParameters(sourceFile, targetDir);

            Logger.Info($"Extracting '{sourceFile}' to '{targetDir}'...");

            //Resets the accumulator
            totalExtractedSize = 0;

            using (var archive = ZipFile.OpenRead(sourceFile))
            {
                var processedEntries = 0;
                var totalEntries = archive.Entries.Count;

                foreach (var entry in archive.Entries)
                {
                    token.ThrowIfCancellationRequested();

                    if (ShouldSkipEntry(component, entry))
                    {
                        Logger.Debug($"Skipping removed entry: {entry.FullName}");
                        continue;
                    }

                    ProcessEntry(entry, targetDir, ignoreUnauthorizedAccess);

                    ReportProgress(++processedEntries, totalEntries);
                }
            }

            Logger.Info("Extraction completed successfully.");
            return Task.FromResult(true);
        }

        private void ValidateZipParameters(string sourceFile, string targetDir)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(ValidateZipParameters), nameof(sourceFile), sourceFile, nameof(targetDir), targetDir);

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(targetDir))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' or 'TargetDir' parameter for zip component.");
            }
        }

        private bool ShouldSkipEntry(Component component, ZipArchiveEntry entry)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(ShouldSkipEntry), nameof(component), component, nameof(entry), entry);

            return component.RemoveItems.Any(removeItem => MatchesPattern(entry.FullName.Replace("/", "\\"), removeItem));
        }

        private void ProcessEntry(ZipArchiveEntry entry, string targetDir, bool ignoreUnauthorizedAccess)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(ProcessEntry), nameof(entry), entry, nameof(targetDir), targetDir, nameof(ignoreUnauthorizedAccess), ignoreUnauthorizedAccess);

            var destinationPath = Path.Combine(targetDir, entry.FullName);

            // Get canonical (absolute) paths to avoid Zip Slip vulnerability
            var canonicalDestinationPath = Path.GetFullPath(destinationPath);
            var canonicalTargetDir = Path.GetFullPath(targetDir);

            // Ensure the destination path is within the target directory
            if (!canonicalDestinationPath.StartsWith(canonicalTargetDir, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Entry is trying to extract outside of the target directory: {entry.FullName}");
            }

            if (IsDirectory(entry))
            {
                Directory.CreateDirectory(canonicalDestinationPath);
            }
            else
            {
                // Update total extracted size
                totalExtractedSize += entry.Length;

                // If the total extracted size exceeds the allowed limit, stop extraction
                if (totalExtractedSize > _maxAllowedUncompressedSize)
                {
                    throw new InvalidOperationException($"Total extracted size exceeds the maximum allowed size of {_maxAllowedUncompressedSize / (1024 * 1024 * 1024)} GB. Possible Zip Bomb attack detected.");
                }

                EnsureDirectoryExists(Path.GetDirectoryName(canonicalDestinationPath));

                if (ignoreUnauthorizedAccess)
                {
                    TryExtractFile(entry, canonicalDestinationPath);
                }
                else
                {
                    ExtractFile(entry, canonicalDestinationPath, true);
                }
            }
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(EnsureDirectoryExists), nameof(directoryPath), directoryPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void TryExtractFile(ZipArchiveEntry entry, string destinationPath)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(TryExtractFile), nameof(entry), entry, nameof(destinationPath), destinationPath);

            try
            {
                ExtractFile(entry, destinationPath, true); // Overwrite existing files
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Warn($"Impossible to create/override the file: '{destinationPath}'");
            }
        }

        protected virtual void ExtractFile(ZipArchiveEntry entry, string destinationPath, bool overwrite)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(ExtractFile), nameof(entry), entry, nameof(destinationPath), destinationPath, nameof(overwrite), overwrite);

            if (entry.Length > _maxAllowedUncompressedSize)
            {
                throw new InvalidOperationException($"File '{entry.FullName}' exceeds the maximum allowed uncompressed size of {_maxAllowedUncompressedSize / (1024 * 1024 * 1024)} GB.");
            }

            entry.ExtractToFile(destinationPath, overwrite);
        }

        private void ReportProgress(int processedEntries, int totalEntries)
        {
            int percentage = (int)((double)processedEntries / totalEntries * 100);
            OnProgress(new PluginProgressEventArgs(percentage, $"Extracted {processedEntries} of {totalEntries} entries"));
        }

        private bool MatchesPattern(string path, string pattern)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(MatchesPattern), nameof(path), path, nameof(pattern), pattern);

            // Escape special characters in the pattern
            string escapedPattern = Regex.Escape(pattern);

            // Replace wildcard characters with their regular expression equivalents
            string regexPattern = "^" + escapedPattern
                                        .Replace("\\*", ".*") // * matches zero or more characters
                                        .Replace("\\?", ".")  // ? matches any single character
                                 + "$";

            try
            {
                // Set a regex timeout to prevent potential DoS attacks
                var matchTimeout = TimeSpan.FromMilliseconds(500);

                // Perform the regular expression match (case-insensitive) with timeout
                return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase, matchTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                Logger.Warn($"Regex match timed out for pattern: {pattern}");
                return false;
            }
        }

        private bool IsDirectory(ZipArchiveEntry entry)
        {
            Logger.TraceMethod(nameof(ZipComponentHandler), nameof(IsDirectory), nameof(entry), entry);

            if (entry == null)
            {
                return false;
            }
            return entry.FullName.Length > 0 && (entry.FullName[entry.FullName.Length - 1] == '/' || entry.FullName[entry.FullName.Length - 1] == '\\');
        }
    }
}
