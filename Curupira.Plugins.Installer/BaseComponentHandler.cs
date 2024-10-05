using Curupira.Plugins.Contract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public abstract class BaseComponentHandler : IComponentHandler
    {
        public event EventHandler<PluginProgressEventArgs> Progress;

        protected ILogProvider Logger { get; private set; }

        protected BaseComponentHandler(ILogProvider logger)
        {
            Logger = logger;
        }

        public abstract Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token);

        protected static string GetAdditionalParams(Component component)
        {
            const string paramsKey = "Params";
            return component.Parameters.ContainsKey(paramsKey) ? component.Parameters[paramsKey] : "";
        }

        protected virtual void OnProgress(PluginProgressEventArgs e)
        {
            Logger.Debug($"{nameof(ZipComponentHandler)}: Progress: {e.Percentage}% - {e.Message}");
            Progress?.Invoke(this, e);
        }
    }
}
