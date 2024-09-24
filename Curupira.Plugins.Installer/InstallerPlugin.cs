using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Installer
{
    public class InstallerPlugin : BasePlugin<InstallerPluginConfig>
    {
        private volatile bool _killed;

        public InstallerPlugin(ILogProvider logger, IPluginConfigParser<InstallerPluginConfig> configParser)
            : base("Installer Plugin", logger, configParser)
        {
        }

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(Execute), nameof(commandLineArgs), commandLineArgs);

            return ExecuteAsync(commandLineArgs).GetAwaiter().GetResult();
        }

        public override async Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            _killed = false;
            var componentId = commandLineArgs != null && commandLineArgs.ContainsKey("component") ? commandLineArgs["component"] : null;
            Component selectedComponent = null;

            if (!string.IsNullOrEmpty(componentId))
            {
                selectedComponent = Config.Components.FirstOrDefault(p => p.Id.Equals(componentId, StringComparison.CurrentCultureIgnoreCase));

                if (selectedComponent == null)
                {
                    Logger.Fatal(FormatLogMessage(nameof(ExecuteAsync), $"Component '{componentId}' not found."));
                    return false;
                }
            }

            var ignoreUnauthorizedAccess = false;

            if (commandLineArgs.TryGetValue("ignoreUnauthorizedAccess", out string ignoreUnauthorizedAccessString))
            {
                if (!string.IsNullOrEmpty(ignoreUnauthorizedAccessString))
                {
                    ignoreUnauthorizedAccess = bool.Parse(ignoreUnauthorizedAccessString.Trim().ToLower());
                }
            }

            bool sucess = true;
            var components = selectedComponent != null ? new[] { selectedComponent } : Config.Components;
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

        private bool HandleZipComponent(Component component, bool ignoreUnauthorizedAccess = false)
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
                                entry.ExtractToFile(destinationPath, true); // Overwrite existing files
                            }
                            catch (UnauthorizedAccessException)
                            {
                                Logger.Warn($"Impossible to create/override the file: '{destinationPath}'");
                            }
                        }
                        else
                        {
                            entry.ExtractToFile(destinationPath, true); // Overwrite existing files
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

        private bool IsDirectory(ZipArchiveEntry entry)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(IsDirectory), nameof(entry), entry);

            if (entry == null)
            {
                return false;
            }
            return entry.FullName.Length > 0 && (entry.FullName[entry.FullName.Length - 1] == '/' || entry.FullName[entry.FullName.Length - 1] == '\\');
        }

        private async Task<bool> HandleMsiComponentAsync(Component component)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(HandleMsiComponentAsync), nameof(component), component);

            string sourceFile = component.Parameters["SourceFile"];
            if (string.IsNullOrEmpty(sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for msi component.");
            }

            string action = component.Action == ComponentAction.Install ? "/i" : "/x";
            string additionalParams = component.Parameters.ContainsKey("Params") ? component.Parameters["Params"] : "";

            string command = $"msiexec.exe {action} \"{sourceFile}\" {additionalParams}";

            Logger.Info($"Executing MSI command: {command}");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    WorkingDirectory = new FileInfo(sourceFile).Directory.FullName,
                    Arguments = $"{action} \"{sourceFile}\" {additionalParams}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();

                // Read output and error streams asynchronously
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);

                string output = await outputTask;
                string error = await errorTask;

                if (process.ExitCode == 0)
                {
                    Logger.Info($"MSI execution completed successfully. Output: {output}");
                }
                else
                {
                    Logger.Error($"MSI execution failed with exit code {process.ExitCode}. Error: {error}");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> HandleBatOrExeComponentAsync(Component component)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(HandleBatOrExeComponentAsync), nameof(component), component);

            string sourceFile = component.Parameters["SourceFile"];
            if (string.IsNullOrEmpty(sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for bat/exe component.");
            }

            string additionalParams = component.Parameters.ContainsKey("Params") ? component.Parameters["Params"] : "";

            string command = $"\"{sourceFile}\" {additionalParams}";

            Logger.Info($"Executing command: {command}");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = sourceFile,
                    WorkingDirectory = new FileInfo(sourceFile).Directory.FullName,
                    Arguments = additionalParams,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);

                string output = await outputTask;
                string error = await errorTask;

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Execution completed successfully. Output: {output}");
                }
                else
                {
                    Logger.Error($"Execution failed with exit code {process.ExitCode}. Error: {error}");
                    return false;
                }
            }

            return true;
        }

        public override bool Kill()
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(Kill));

            _killed = true;
            return true;
        }

        public override void Dispose()
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(Dispose));
            // This plugin doesn't have any resources to dispose.
        }
    }
}