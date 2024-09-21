using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.FoldersCreator
{
    /// <summary>
    /// Plugin to create a list of folders if they do not already exist.
    /// </summary>
    public class FoldersCreatorPlugin : BasePlugin<FoldersCreatorPluginConfig>
    {
        private volatile bool _killed = false;

        public FoldersCreatorPlugin(ILogProvider logger, IPluginConfigParser<FoldersCreatorPluginConfig> configParser)
            : base("Folders Creator Plugin", logger, configParser)
        {
        }

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            _killed = false;
            var success = true;
            try
            {
                int totalDirectories = Config.DirectoriesToCreate.Count;
                int processedDirectories = 0;

                foreach (string directoryPath in Config.DirectoriesToCreate)
                {
                    if (_killed)
                    {
                        Logger.Info(FormatLogMessage(nameof(Execute), "Plugin execution cancelled."));
                        success = false;
                        break;
                    }
                    else
                    {
                        if (!Directory.Exists(directoryPath))
                        {
                            string existingDirectory = FileSystemHelper.GetFirstExistingDirectoryOrRoot(directoryPath);

                            if (string.IsNullOrEmpty(existingDirectory))
                            {
                                Logger.Error(FormatLogMessage(nameof(Execute), $"Invalid path '{directoryPath}'"));
                                success = false;
                            }
                            else if (directoryPath.StartsWith(@"\\"))
                            {
                                try
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                catch (IOException)
                                {
                                    Logger.Error(FormatLogMessage(nameof(Execute), $"An error occurred during network directory creation '{directoryPath}'"));
                                    success = false;
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    Logger.Error(FormatLogMessage(nameof(Execute), $"Insufficient permissions to create directory '{directoryPath}'"));
                                    success = false;
                                }
                            }
                            else if (HasCreateDirectoryPermission(existingDirectory))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            else
                            {
                                Logger.Error(FormatLogMessage(nameof(Execute), $"Insufficient permissions to create directory '{directoryPath}'"));
                                success = false;
                            }
                        }

                        processedDirectories++;
                        int percentage = (int)((double)processedDirectories / totalDirectories * 100);
                        OnProgress(new PluginProgressEventArgs(percentage, $"Processed {processedDirectories} of {totalDirectories} directories"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, FormatLogMessage(nameof(Execute), "An error occurred during directory creation."));
                success = false;
            }
            return success;
        }

        public override bool Kill()
        {
            _killed = true;
            return true;
        }

        /// <summary>
        /// Checks if the current user has permission to create a directory at the specified path
        /// </summary>
        /// <param name="directoryPath">The path to the directory to check</param>
        /// <returns>True if the user has permission, false otherwise</returns>
        private bool HasCreateDirectoryPermission(string directoryPath)
        {
            try
            {
                // Get the current Windows identity and the user's principal
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(currentUser);

                // Get the access control information for the directory
                DirectorySecurity directorySecurity = Directory.GetAccessControl(directoryPath);

                // Get the access rules (user and group access)
                AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    // Check if the rule applies to the current user or one of the user's groups
                    if (principal.IsInRole(new SecurityIdentifier(rule.IdentityReference.Value)) &&
                        rule.AccessControlType == AccessControlType.Allow)
                    {
                        // Check for CreateDirectories or Write rights using bitwise AND
                        if ((rule.FileSystemRights & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories ||
                            (rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                        {
                            return true;
                        }
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

        public override void Dispose()
        {
            // This plugin doesn't have any resources to dispose.
        }
    }
}