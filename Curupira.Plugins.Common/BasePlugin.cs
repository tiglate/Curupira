using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Common
{
    public abstract class BasePlugin<TPluginConfig> : IPlugin
        where TPluginConfig : class
    {
        protected ILogProvider Logger { get; }
        private readonly IPluginConfigParser<TPluginConfig> _configParser;

        protected BasePlugin(string pluginName, ILogProvider logger, IPluginConfigParser<TPluginConfig> configParser)
        {
            Name = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configParser = configParser ?? throw new ArgumentNullException(nameof(configParser));
        }

        public virtual TPluginConfig Config { get; private set; }

        public virtual string Name { get; private set; }

        public virtual bool Execute(IDictionary<string, string> commandLineArgs)
        {
            return true;
        }

        public virtual bool Kill()
        {
            return true;
        }

        public event EventHandler<PluginProgressEventArgs> Progress;

        public virtual void Init()
        {
            Logger.TraceMethod(nameof(BasePlugin<TPluginConfig>), nameof(Init));

            Config = _configParser.Execute();
        }

        public virtual async Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs)
        {
            Logger.TraceMethod(nameof(BasePlugin<TPluginConfig>), nameof(ExecuteAsync), nameof(commandLineArgs), commandLineArgs);

            try
            {
                Logger.Debug(FormatLogMessage(nameof(ExecuteAsync), "Executing plugin logic."));
                return await Task.Run(() => Execute(commandLineArgs)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, FormatLogMessage(nameof(ExecuteAsync), "An error occurred during plugin execution."));
                return false;
            }
        }

        public virtual async Task<bool> KillAsync()
        {
            Logger.TraceMethod(nameof(BasePlugin<TPluginConfig>), nameof(KillAsync));

            try
            {
                Logger.Debug(FormatLogMessage(nameof(KillAsync), "Attempting to kill plugin."));
                return await Task.Run(() => Kill()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, FormatLogMessage(nameof(KillAsync), "An error occurred during plugin kill."));
                return false;
            }
        }

        protected virtual void OnProgress(PluginProgressEventArgs e)
        {
            Logger.Debug(FormatLogMessage(nameof(OnProgress), $"Progress: {e.Percentage}% - {e.Message}"));
            Progress?.Invoke(this, e);
        }

        protected virtual string FormatLogMessage(string method, string message, bool includeTimestamp = false)
        {
            var formattedMessage = $"{Name}->{method}: {message}";
            if (includeTimestamp)
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                formattedMessage = $"[{now}] {formattedMessage}";
            }
            return formattedMessage;
        }

        public abstract void Dispose();
    }
}