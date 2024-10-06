using Autofac;
using Curupira.Plugins.Contract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.AppClient.Services
{
    public class PluginExecutor : IPluginExecutor
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogProvider _logger;
        private readonly IProgressBarService _progressBarService;
        private readonly IConsoleService _consoleService;

        public PluginExecutor(ILifetimeScope scope, ILogProvider logger, IProgressBarService progressBarService, IConsoleService consoleService)
        {
            _scope = scope;
            _logger = logger;
            _progressBarService = progressBarService;
            _consoleService = consoleService;
        }

        public async Task<bool> ExecutePluginAsync(Options options, CancellationToken cancellationToken = default)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(ExecutePluginAsync), nameof(options), options);

            if (!_scope.IsRegisteredWithName(options.Plugin, typeof(IPlugin)))
            {
                var message = $"Plugin '{options.Plugin}' not found!";
                _consoleService.WriteLine(message);
                _logger.Fatal(message);
                return false;
            }

            using (var plugin = _scope.ResolveNamed<IPlugin>(options.Plugin))
            {
                if (options.NoProgressBar)
                {
                    _consoleService.WriteLine($"Loaded: {plugin.Name}");
                }
                else
                {
                    _consoleService.WriteCentered($"Loaded: {plugin.Name}");
                    _consoleService.WriteLine();
                }

                if (!TryInitializePlugin(plugin, options.Plugin))
                {
                    return false;
                }

                AttachProgressHandler(plugin, options.NoProgressBar);

                var success = await TryExecutePluginAsync(plugin, options, cancellationToken).ConfigureAwait(false);

                if (success)
                {
                    var message = $"Plugin '{plugin.Name}' executed successfully!";
                    _consoleService.WriteLine(message);
                    _logger.Info(message);
                }
                else
                {
                    var message = $"The execution of '{plugin.Name}' plugin failed.";
                    _consoleService.WriteLine(message);
                    _logger.Info(message);
                    _logger.Error(message);
                }

                return success;
            }
        }

        protected virtual void AttachProgressHandler(IPlugin plugin, bool noProgressBar)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(AttachProgressHandler), nameof(plugin), plugin, nameof(noProgressBar), noProgressBar);

            if (noProgressBar)
            {
                var lastReportedProgress = -1;
                plugin.Progress += (sender, e) =>
                {
                    int currentProgress = (int)Math.Floor(e.Percentage / 10.0) * 10;

                    if (currentProgress > lastReportedProgress)
                    {
                        _logger.Info($"[{currentProgress}%] {e.Message}");
                        lastReportedProgress = currentProgress == 100 ? -1 : currentProgress;
                    }
                };
            }
            else
            {
                _progressBarService.Init(10000, "Loading");

                plugin.Progress += (sender, e) =>
                {
                    _progressBarService.SetMessage(e.Message);
                    _progressBarService.ReportProgress(e.Percentage / 100f);
                };

                _progressBarService.SetMessage($"Loading plugin: {plugin.Name}...");
            }
        }

        protected virtual bool TryInitializePlugin(IPlugin plugin, string pluginName)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(TryInitializePlugin), nameof(plugin), plugin, nameof(pluginName), pluginName);

            try
            {
                plugin.Init();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error when initializing the plugin '{pluginName}'");
                return false;
            }
        }

        protected virtual Task<bool> TryExecutePluginAsync(IPlugin plugin, Options options, CancellationToken cancellationToken)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(TryExecutePluginAsync), nameof(plugin), plugin, nameof(options), options);

            try
            {
                var pluginParams = ParseParams(options.Params);
                return plugin.ExecuteAsync(pluginParams, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error when executing the plugin '{options.Plugin}'");
                return Task.FromResult(false);
            }
        }

        // Helper method to convert string parameters to IDictionary<string, string>
        protected virtual IDictionary<string, string> ParseParams(string paramString)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(ParseParams), nameof(paramString), paramString);

            var paramDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(paramString))
            {
                return paramDict;
            }

            // Split parameters by spaces, assuming format is "key=value"
            var tokens = paramString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                var keyValue = token.Split(new[] { '=' }, 2); // Split only on the first '='

                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    // Add to the dictionary
                    paramDict[key] = value;
                }
                else
                {
                    throw new FormatException($"Invalid parameter format '{token}'. Expected 'key=value'.");
                }
            }

            return paramDict;
        }
    }
}
