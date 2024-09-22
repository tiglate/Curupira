using Autofac;
using Curupira.Plugins.Contract;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curupira.AppClient
{
    public class PluginExecutor : IPluginExecutor
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogProvider _logProvider;

        public PluginExecutor(ILifetimeScope scope, ILogProvider logProvider)
        {
            _scope = scope;
            _logProvider = logProvider;
        }

        public async Task<bool> ExecutePluginAsync(Options options)
        {
            if (!_scope.IsRegisteredWithName(options.Plugin, typeof(IPlugin)))
            {
                var message = $"Plugin 'options.Plugin' not found!";
                Console.WriteLine(message);
                _logProvider.Fatal(message);
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
                    ConsoleHelper.WriteCentered($"Loaded: {plugin.Name}");
                    Console.WriteLine();

                    plugin.Init();

                    progressBar = new ProgressBar(10000, "Loading", barSettings);

                    plugin.Progress += (sender, e) =>
                    {
                        progressBar.Message = e.Message;
                        var progress = progressBar.AsProgress<float>();
                        progress.Report(e.Percentage / 100f);
                    };

                    progressBar.Message = $"Loading plugin: {plugin.Name}...";

                    var pluginParams = ParseParams(options.Params);

                    var success = await plugin.ExecuteAsync(pluginParams).ConfigureAwait(false);

                    progressBar.Dispose();
                    progressBar = null;

                    if (success)
                    {
                        var message = $"Plugin '{plugin.Name}' executed successfully!";
                        Console.WriteLine(message);
                        _logProvider.Info(message);
                    }
                    else
                    {
                        var message = $"The execution of '{plugin.Name}' plugin failed.";
                        Console.WriteLine(message);
                        _logProvider.Info(message);
                        _logProvider.Error(message);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logProvider.Error(ex, $"Error when executing the plugin '{options.Plugin}'");
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
