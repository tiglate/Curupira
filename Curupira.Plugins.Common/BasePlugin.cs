using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Curupira.Plugins.Contract;

namespace Curupira.Plugins.Common
{
    public abstract class BasePlugin : IPlugin
    {
        protected ILogProvider Logger { get; }

        protected BasePlugin(string pluginName, ILogProvider logger)
        {
            Name = pluginName;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual string Name { get; private set; }

        public abstract void Init(XmlElement config);

        public abstract bool Execute(IDictionary<string, string> commandLineArgs);

        public abstract bool Kill();

        public event EventHandler<PluginProgressEventArgs> Progress;

        public IAsyncResult BeginExecute(IDictionary<string, string> commandLineArgs, AsyncCallback callback, object state)
        {
            Logger.Trace(FormatLogMessage(nameof(BeginExecute), "method called."));

            var pluginAsyncResult = new PluginAsyncResult(false, state);
            var thread = new Thread(obj =>
            {
                var asyncResult = (PluginAsyncResult)obj;
                try
                {
                    Logger.Debug(FormatLogMessage(nameof(BeginExecute), "Executing plugin logic on a separate thread."));
                    bool result = Execute(commandLineArgs);
                    asyncResult.Complete(result);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, FormatLogMessage(nameof(BeginExecute), "An error occurred during plugin execution."));
                    asyncResult.Complete(ex);
                }
                finally
                {
                    callback?.Invoke(asyncResult);
                }
            });

            thread.Start(pluginAsyncResult);
            return pluginAsyncResult;
        }

        public bool EndExecute(IAsyncResult asyncResult)
        {
            Logger.Trace(FormatLogMessage(nameof(EndExecute), "method called."));
            return ((PluginAsyncResult)asyncResult).Result;
        }

        public IAsyncResult BeginKill(AsyncCallback callback, object state)
        {
            Logger.Trace(FormatLogMessage(nameof(BeginKill), "method called."));

            var executeAsyncResult = new PluginAsyncResult(false, state);
            var thread = new Thread(obj =>
            {
                var asyncResult = (PluginAsyncResult)obj;
                try
                {
                    Logger.Debug(FormatLogMessage(nameof(BeginKill), "Attempting to kill plugin on a separate thread."));
                    bool result = Kill();
                    asyncResult.Complete(result);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, FormatLogMessage(nameof(BeginKill), "An error occurred during plugin kill."));
                    asyncResult.Complete(ex);
                }
                finally
                {
                    callback?.Invoke(asyncResult);
                }
            });

            thread.Start(executeAsyncResult);
            return executeAsyncResult;
        }

        public bool EndKill(IAsyncResult asyncResult)
        {
            Logger.Trace(FormatLogMessage(nameof(EndKill), "method called."));
            return ((PluginAsyncResult)asyncResult).Result;
        }

        protected virtual void OnProgress(PluginProgressEventArgs e)
        {
            Logger.Info(FormatLogMessage(nameof(OnProgress), $"Progress: {e.Percentage}% - {e.Message}"));
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