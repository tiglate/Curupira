using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Installer
{
    public class InstallerPlugin : BasePlugin<InstallerPluginConfig>
    {
        private readonly Dictionary<ComponentType, IComponentHandler> _handlers;

        public InstallerPlugin(ILogProvider logger, IPluginConfigParser<InstallerPluginConfig> configParser, IProcessExecutor processExecutor)
            : base("Installer Plugin", logger, configParser)
        {
            _handlers = new Dictionary<ComponentType, IComponentHandler>
            {
                { ComponentType.Zip, new ZipComponentHandler(logger) },
                { ComponentType.Msi, new MsiComponentHandler(processExecutor, logger) },
                { ComponentType.Bat, new BatOrExeComponentHandler(processExecutor, logger) },
                { ComponentType.Exe, new BatOrExeComponentHandler(processExecutor, logger) }
            };

            foreach (var handler in _handlers.Values)
            {
                handler.Progress += (sender, e) => OnProgress(e);
            }
        }

        public override async Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs, CancellationToken cancelationToken = default)
        {
            Logger.TraceMethod(nameof(InstallerPlugin), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            var components = GetComponents(commandLineArgs);

            if (components == null || components.Count == 0)
            {
                return false;
            }

            var ignoreUnauthorizedAccess = GetIgnoreUnauthorizedAccessFlag(commandLineArgs);
            var success = true;
            var processedComponents = 0;
            var totalComponents = components.Count;

            foreach (var component in components)
            {
                cancelationToken.ThrowIfCancellationRequested();

                if (_handlers.TryGetValue(component.Type, out var handler))
                {
                    try
                    {
                        success = success && await handler.HandleAsync(component, ignoreUnauthorizedAccess, cancelationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Info(FormatLogMessage(nameof(ExecuteAsync), "Plugin execution cancelled."));
                        success = false;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "An error occurred during installation/uninstallation.");
                        success = false;
                    }

                    processedComponents++;
                    int percentage = (int)((double)processedComponents / totalComponents * 100);
                    OnProgress(new PluginProgressEventArgs(percentage, $"Processed {processedComponents} of {totalComponents} components"));
                }
                else
                {
                    throw new NotSupportedException($"Unsupported component type: {component.Type}");
                }
            }

            return success;
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
                    return new List<Component>();
                }
            }

            return selectedComponent != null ? new[] { selectedComponent } : Config.Components;
        }

        private static bool GetIgnoreUnauthorizedAccessFlag(IDictionary<string, string> commandLineArgs)
        {
            return commandLineArgs.TryGetValue("ignoreUnauthorizedAccess", out string ignoreUnauthorizedAccessString)
                && bool.TryParse(ignoreUnauthorizedAccessString, out bool result) && result;
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