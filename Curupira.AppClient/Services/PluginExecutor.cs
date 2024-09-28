using Autofac;
using Curupira.Plugins.Contract;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curupira.AppClient.Services
{
    public class PluginExecutor : IPluginExecutor
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogProvider _logger;

        public PluginExecutor(ILifetimeScope scope, ILogProvider logger)
        {
            _scope = scope;
            _logger = logger;
        }

        public async Task<bool> ExecutePluginAsync(Options options)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(ExecutePluginAsync), nameof(options), options);

            if (!_scope.IsRegisteredWithName(options.Plugin, typeof(IPlugin)))
            {
                var message = $"Plugin '{options.Plugin}' not found!";
                Console.WriteLine(message);
                _logger.Fatal(message);
                return false;
            }

            var barSettings = new ProgressBarOptions
            {
                CollapseWhenFinished = true,
                EnableTaskBarProgress = true,
            };

            ProgressBar progressBar = null;

            try
            {
                using (var plugin = _scope.ResolveNamed<IPlugin>(options.Plugin))
                {
                    if (options.NoProgressBar)
                    {
                        Console.WriteLine($"Loaded: {plugin.Name}");
                    }
                    else
                    {
                        ConsoleHelper.WriteCentered($"Loaded: {plugin.Name}");
                        Console.WriteLine();
                    }

                    plugin.Init();

                    if (options.NoProgressBar)
                    {
                        var lastReportedProgress = -1;

                        plugin.Progress += (sender, e) =>
                        {
                            int currentProgress = (int)Math.Floor(e.Percentage / 10.0) * 10; // Round down to the nearest 10%

                            if (currentProgress > lastReportedProgress) // Only report if progress has increased by at least 10%
                            {
                                _logger.Info($"[{currentProgress}%] {e.Message}");
                                lastReportedProgress = currentProgress == 100 ? -1 : currentProgress;
                            }
                        };
                    }
                    else
                    {
                        progressBar = new ProgressBar(10000, "Loading", barSettings);

                        plugin.Progress += (sender, e) =>
                        {
                            progressBar.Message = e.Message;
                            var progress = progressBar.AsProgress<float>();
                            progress.Report(e.Percentage / 100f);
                        };

                        progressBar.Message = $"Loading plugin: {plugin.Name}...";
                    }

                    var pluginParams = ParseParams(options.Params);

                    var success = await plugin.ExecuteAsync(pluginParams).ConfigureAwait(false);

                    progressBar?.Dispose();
                    progressBar = null;

                    if (success)
                    {
                        var message = $"Plugin '{plugin.Name}' executed successfully!";
                        Console.WriteLine(message);
                        _logger.Info(message);
                    }
                    else
                    {
                        var message = $"The execution of '{plugin.Name}' plugin failed.";
                        Console.WriteLine(message);
                        _logger.Info(message);
                        _logger.Error(message);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error when executing the plugin '{options.Plugin}'");
                return false;
            }
            finally
            {
                progressBar?.Dispose();
            }
        }

        // Helper method to convert string parameters to IDictionary<string, string>
        private IDictionary<string, string> ParseParams(string paramString)
        {
            _logger.TraceMethod(nameof(PluginExecutor), nameof(ParseParams), nameof(paramString), paramString);

            var paramDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(paramString))
            {
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
                        Console.WriteLine($"Warning: Invalid parameter format '{token}'. Expected 'key=value'.");
                    }
                }
            }

            return paramDict;
        }
    }
}
