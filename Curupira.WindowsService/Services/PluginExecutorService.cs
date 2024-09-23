using Autofac;
using Curupira.Plugins.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curupira.WindowsService.Services
{
    public class PluginExecutorService : IPluginExecutorService
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogProvider _logProvider;

        public PluginExecutorService(ILifetimeScope scope, ILogProvider logProvider)
        {
            _scope = scope;
            _logProvider = logProvider;
        }

        public async Task<bool> ExecutePluginAsync(string pluginName, IDictionary<string, string> pluginParams)
        {
            if (!_scope.IsRegisteredWithName(pluginName, typeof(IPlugin)))
            {
                _logProvider.Error($"Plugin '{pluginName}' not found!");
                return false;
            }

            try
            {
                using (var plugin = _scope.ResolveNamed<IPlugin>(pluginName))
                {
                    plugin.Init();

                    return await plugin.ExecuteAsync(pluginParams).ConfigureAwait(false);

                }
            }
            catch (Exception ex)
            {
                _logProvider.Error(ex, $"Error when executing the plugin '{pluginName}'");
                return false;
            }
        }
    }
}
