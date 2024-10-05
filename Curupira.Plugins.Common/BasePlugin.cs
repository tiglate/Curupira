using System;
using System.Collections.Generic;
using System.Threading;
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

        public virtual TPluginConfig Config { get; protected set; }

        public virtual string Name { get; private set; }

        public event EventHandler<PluginProgressEventArgs> Progress;

        public virtual void Init()
        {
            Logger.TraceMethod(nameof(BasePlugin<TPluginConfig>), nameof(Init));

            Config = _configParser.Execute();
        }

        public abstract Task<bool> ExecuteAsync(IDictionary<string, string> commandLineArgs, CancellationToken cancelationToken = default);

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}