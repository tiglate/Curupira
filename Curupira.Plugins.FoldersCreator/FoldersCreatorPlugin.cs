using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.FoldersCreator
{
    /// <summary>
    /// Plugin to create a list of folders if they do not already exist.
    /// </summary>
    public class FoldersCreatorPlugin : BasePlugin<FoldersCreatorPluginConfig>
    {
        public FoldersCreatorPlugin(ILogProvider logger, IPluginConfigParser<FoldersCreatorPluginConfig> configParser)
            : base("FoldersCreatorPlugin", logger, configParser)
        {
        }

        public override Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs, CancellationToken cancelationToken = default)
        {
            Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            var success = true;

            try
            {
                int totalDirectories = Config.DirectoriesToCreate.Count;
                int processedDirectories = 0;

                foreach (string directoryPath in Config.DirectoriesToCreate)
                {
                    cancelationToken.ThrowIfCancellationRequested();

                    if (!ProcessDirectory(directoryPath))
                    {
                        success = false;
                    }

                    processedDirectories++;
                    ReportProgress(processedDirectories, totalDirectories);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info(FormatLogMessage(nameof(ExecuteAsync), "Plugin execution cancelled."));
                success = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, FormatLogMessage(nameof(ExecuteAsync), "An error occurred during directory creation."));
                success = false;
            }

            return Task.FromResult(success);
        }

        private bool ProcessDirectory(string directoryPath)
        {
            Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(ProcessDirectory), nameof(directoryPath), directoryPath);

            if (DirectoryExists(directoryPath)) return true;

            string existingDirectory = FileSystemHelper.GetFirstExistingDirectoryOrRoot(directoryPath);

            if (string.IsNullOrEmpty(existingDirectory))
            {
                Logger.Error(FormatLogMessage(nameof(ProcessDirectory), $"Invalid path '{directoryPath}'"));
                return false;
            }

            if (directoryPath.StartsWith(@"\\")) return TryCreateNetworkDirectory(directoryPath);

            return TryCreateLocalDirectory(directoryPath, existingDirectory);
        }

        private bool TryCreateNetworkDirectory(string directoryPath)
        {
            Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(TryCreateNetworkDirectory), nameof(directoryPath), directoryPath);

            try
            {
                CreateDirectory(directoryPath);
                return true;
            }
            catch (IOException)
            {
                Logger.Error(FormatLogMessage(nameof(TryCreateNetworkDirectory), $"An error occurred during network directory creation '{directoryPath}'"));
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error(FormatLogMessage(nameof(TryCreateNetworkDirectory), $"Insufficient permissions to create directory '{directoryPath}'"));
            }
            return false;
        }

        private bool TryCreateLocalDirectory(string directoryPath, string existingDirectory)
        {
            Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(TryCreateLocalDirectory), nameof(directoryPath), directoryPath, nameof(existingDirectory), existingDirectory);

            if (!HasCreateDirectoryPermission(existingDirectory))
            {
                Logger.Error(FormatLogMessage(nameof(TryCreateLocalDirectory), $"Insufficient permissions to create directory '{directoryPath}'"));
                return false;
            }

            CreateDirectory(directoryPath);
            return true;
        }

        private void ReportProgress(int processedDirectories, int totalDirectories)
        {
            int percentage = (int)((double)processedDirectories / totalDirectories * 100);
            OnProgress(new PluginProgressEventArgs(percentage, $"Processed {processedDirectories} of {totalDirectories} directories"));
        }

        /// <summary>
        /// Checks if the current user has permission to create a directory at the specified path
        /// </summary>
        /// <param name="directoryPath">The path to the directory to check</param>
        /// <returns>True if the user has permission, false otherwise</returns>
        protected virtual bool HasCreateDirectoryPermission(string directoryPath)
        {
            Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(HasCreateDirectoryPermission), nameof(directoryPath), directoryPath);

            try
            {
                // Get the current Windows identity and the user's principal
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(currentUser);

                // Get the access control information for the directory
                DirectorySecurity directorySecurity = GetAccessControl(directoryPath);

                // Get the access rules (user and group access)
                AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    // Check if the rule applies to the current user or one of the user's groups
                    if (principal.IsInRole(new SecurityIdentifier(rule.IdentityReference.Value)) &&
                        rule.AccessControlType == AccessControlType.Allow &&
                        ((rule.FileSystemRights & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories ||
                         (rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error("UnauthorizedAccessException: No permission to access the directory {0}", directoryPath);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while checking directory permissions.");
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.TraceMethod(nameof(FoldersCreatorPlugin), nameof(Dispose));
            }
            // This plugin doesn't have any resources to dispose.
        }

        #region Wrappers to be mocked by unit tests

        protected virtual DirectoryInfo CreateDirectory(string directoryPath)
        {
            return Directory.CreateDirectory(directoryPath);
        }

        protected virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        protected virtual DirectorySecurity GetAccessControl(string directoryPath)
        {
            return Directory.GetAccessControl(directoryPath);
        }

        #endregion
    }
}