using Curupira.Plugins.Contract;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public class MsiComponentHandler : IComponentHandler
    {
        private readonly IProcessExecutor _processExecutor;
        private readonly ILogProvider _logger;

        public MsiComponentHandler(IProcessExecutor processExecutor, ILogProvider logger)
        {
            _processExecutor = processExecutor;
            _logger = logger;
        }

        public event EventHandler<PluginProgressEventArgs> Progress;

        public async Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token)
        {
            if (!component.Parameters.TryGetValue("SourceFile", out string sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for MSI component.");
            }

            var action = component.Action == ComponentAction.Install ? "/i" : "/x";
            var additionalParams = GetAdditionalParams(component);

            OnProgress(new PluginProgressEventArgs(0, "Executing..."));

            var exitCode = await _processExecutor.ExecuteAsync("msiexec.exe", $"{action} \"{sourceFile}\" {additionalParams}", new FileInfo(sourceFile).Directory.FullName);

            OnProgress(new PluginProgressEventArgs(0, "Completed."));

            if (exitCode == 0)
            {
                _logger.Info($"MSI execution completed successfully.");
                return true;
            }
            else
            {
                _logger.Error($"MSI execution failed with exit code {exitCode}.");
                return false;
            }
        }

        private static string GetAdditionalParams(Component component)
        {
            const string paramsKey = "Params";
            return component.Parameters.ContainsKey(paramsKey) ? component.Parameters[paramsKey] : "";
        }

        private void OnProgress(PluginProgressEventArgs e)
        {
            _logger.Debug($"{nameof(ZipComponentHandler)}: Progress: {e.Percentage}% - {e.Message}");
            Progress?.Invoke(this, e);
        }
    }
}
