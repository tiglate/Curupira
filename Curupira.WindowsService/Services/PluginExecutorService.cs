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
        private readonly ILogProvider _logger;

        public PluginExecutorService(ILifetimeScope scope, ILogProvider logger)
        {
            _scope = scope;
            _logger = logger;
        }

        public async Task<bool> ExecutePluginAsync(string pluginName, IDictionary<string, string> pluginParams)
        {
            if (!_scope.IsRegisteredWithName(pluginName, typeof(IPlugin)))
            {
                _logger.Error($"Plugin '{pluginName}' not found!");
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
                _logger.Error(ex, $"Error when executing the plugin '{pluginName}'");
                return false;
            }
        }
    }
}
