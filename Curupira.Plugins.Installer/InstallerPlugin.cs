using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Installer
{
    public class InstallerPlugin : BasePlugin<InstallerPluginConfig>
    {
        private volatile bool _killed;
        private readonly IProcessExecutor _processExecutor;

        public InstallerPlugin(ILogProvider logger, IPluginConfigParser<InstallerPluginConfig> configParser, IProcessExecutor processExecutor)
            : base("Installer Plugin", logger, configParser)
        {
            _processExecutor = processExecutor;
        }

        public override async Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            _killed = false;

            var components = GetComponents(commandLineArgs);

            if (components == null)
            {
                return false;
            }

            var ignoreUnauthorizedAccess = GetIgnoreUnauthorizedAccessFlag(commandLineArgs);
            var sucess = true;
            var processedComponents = 0;
            var totalComponents = components.Count;

            foreach (var component in components)
            {
                if (_killed)
                {
                    Logger.Info(FormatLogMessage(nameof(ExecuteAsync), "Plugin execution cancelled."));
                    return false;
                }

                try
                {
                    var auxSuccess = true;

                    switch (component.Type)
                    {
                        case ComponentType.Zip:
                            auxSuccess = HandleZipComponent(component, ignoreUnauthorizedAccess);
                            sucess = sucess && auxSuccess;
                            break;
                        case ComponentType.Msi:
                            auxSuccess = await HandleMsiComponentAsync(component).ConfigureAwait(false);
                            sucess = sucess && auxSuccess;
                            break;
                        case ComponentType.Bat:
                        case ComponentType.Exe:
                            auxSuccess = await HandleBatOrExeComponentAsync(component).ConfigureAwait(false);
                            sucess = sucess && auxSuccess;
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported component type: {component.Type}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "An error occurred during installation/uninstallation.");
                    sucess = false;
                }

                processedComponents++;
                int percentage = (int)((double)processedComponents / totalComponents * 100);
                OnProgress(new PluginProgressEventArgs(percentage, $"Processed {processedComponents} of {totalComponents} components"));
            }

            return sucess;
        }

        private IList<Component> GetComponents(IDictionary<string, string> commandLineArgs)
        {
            var componentId = commandLineArgs != null && commandLineArgs.ContainsKey("component") ? commandLineArgs["component"] : null;
            Component selectedComponent = null;

            if (!string.IsNullOrEmpty(componentId))
            {
                selectedComponent = Config.Components.FirstOrDefault(p => p.Id.Equals(componentId, StringComparison.CurrentCultureIgnoreCase));

                if (selectedComponent == null)
                {
                    Logger.Fatal(FormatLogMessage(nameof(ExecuteAsync), $"Component '{componentId}' not found."));
                    return null;
                }
            }

            return selectedComponent != null ? new[] { selectedComponent } : Config.Components;
        }

        private static bool GetIgnoreUnauthorizedAccessFlag(IDictionary<string, string> commandLineArgs)
        {
            return commandLineArgs.TryGetValue("ignoreUnauthorizedAccess", out string ignoreUnauthorizedAccessString)
                && bool.TryParse(ignoreUnauthorizedAccessString, out bool result) && result;
        }

        protected virtual bool HandleZipComponent(Component component, bool ignoreUnauthorizedAccess = false)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(HandleZipComponent), nameof(component), component, nameof(ignoreUnauthorizedAccess), ignoreUnauthorizedAccess);

            string sourceFile = component.Parameters["SourceFile"];
            string targetDir = component.Parameters["TargetDir"];

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(targetDir))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' or 'TargetDir' parameter for zip component.");
            }

            Logger.Info($"Extracting '{sourceFile}' to '{targetDir}'...");

            using (var archive = ZipFile.OpenRead(sourceFile))
            {
                var processedEntries = 0;
                var totalEntries = archive.Entries.Count;

                foreach (var entry in archive.Entries)
                {
                    if (_killed)
                    {
                        Logger.Info(FormatLogMessage(nameof(HandleZipComponent), "Plugin execution cancelled."));
                        return false;
                    }

                    // Check if the entry should be removed
                    if (component.RemoveItems.Any(removeItem => MatchesPattern(entry.FullName.Replace("/", "\\"), removeItem)))
                    {
                        Logger.Debug($"Skipping removed entry: {entry.FullName}");
                        continue; // Skip this entry
                    }

                    string destinationPath = Path.Combine(targetDir, entry.FullName);

                    if (IsDirectory(entry))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                    else
                    {
                        var auxDir = Path.GetDirectoryName(destinationPath);

                        if (!Directory.Exists(auxDir))
                        {
                            Directory.CreateDirectory(auxDir);
                        }

                        if (ignoreUnauthorizedAccess)
                        {
                            try
                            {
                                ExtractFile(entry, destinationPath, true); // Overwrite existing files
                            }
                            catch (UnauthorizedAccessException)
                            {
                                Logger.Warn($"Impossible to create/override the file: '{destinationPath}'");
                            }
                        }
                        else
                        {
                            ExtractFile(entry, destinationPath, true); // Overwrite existing files
                        }
                    }

                    processedEntries++;
                    int percentage = (int)((double)processedEntries / totalEntries * 100);
                    OnProgress(new PluginProgressEventArgs(percentage, $"Extracted {processedEntries} of {totalEntries} entries"));
                }
            }

            Logger.Info("Extraction completed successfully.");
            return true;
        }

        private static bool MatchesPattern(string path, string pattern)
        {
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

        private bool IsDirectory(ZipArchiveEntry entry)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(IsDirectory), nameof(entry), entry);

            if (entry == null)
            {
                return false;
            }
            return entry.FullName.Length > 0 && (entry.FullName[entry.FullName.Length - 1] == '/' || entry.FullName[entry.FullName.Length - 1] == '\\');
        }

        protected virtual async Task<bool> HandleMsiComponentAsync(Component component)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(HandleMsiComponentAsync), nameof(component), component);

            if (!component.Parameters.TryGetValue("SourceFile", out string sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for msi component.");
            }

            string action = component.Action == ComponentAction.Install ? "/i" : "/x";
            string additionalParams = component.Parameters.ContainsKey("Params") ? component.Parameters["Params"] : "";

            var exitCode = await _processExecutor.ExecuteAsync("msiexec.exe", $"{action} \"{sourceFile}\" {additionalParams}", new FileInfo(sourceFile).Directory.FullName);

            if (exitCode == 0)
            {
                Logger.Info($"MSI execution completed successfully.");
                return true;
            }
            else
            {
                Logger.Error($"MSI execution failed with exit code {exitCode}.");
                return false;
            }
        }

        protected virtual async Task<bool> HandleBatOrExeComponentAsync(Component component)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(HandleBatOrExeComponentAsync), nameof(component), component);

            if (!component.Parameters.TryGetValue("SourceFile", out string sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for bat/exe component.");
            }

            string additionalParams = component.Parameters.ContainsKey("Params") ? component.Parameters["Params"] : "";

            // Build the command for .bat or .exe
            string command = $"\"{sourceFile}\" {additionalParams}";

            Logger.Info($"Executing command: {command}");

            var exitCode = await _processExecutor.ExecuteAsync(sourceFile, additionalParams, new FileInfo(sourceFile).Directory.FullName);

            if (exitCode == 0)
            {
                Logger.Info($"Execution completed successfully.");
                return true;
            }
            else
            {
                Logger.Error($"Execution failed with exit code {exitCode}.");
                return false;
            }
        }

        protected virtual void ExtractFile(ZipArchiveEntry entry, string destinationPath, bool overwrite)
        {
            entry.ExtractToFile(destinationPath, overwrite);
        }

        public override Task<bool> KillAsync()
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(KillAsync));

            _killed = true;
            return Task.FromResult(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.TraceMethod(nameof(InstallerPlugin), nameof(Dispose));
            }
            // This plugin doesn't have any resources to dispose.
        }
    }
}