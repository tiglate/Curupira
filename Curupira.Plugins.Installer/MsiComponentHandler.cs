using Curupira.Plugins.Contract;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public class MsiComponentHandler : BaseComponentHandler
    {
        private readonly IProcessExecutor _processExecutor;

        public MsiComponentHandler(IProcessExecutor processExecutor, ILogProvider logger)
            : base(logger)
        {
            _processExecutor = processExecutor;
        }

        public override async Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token)
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
                Logger.Info($"MSI execution completed successfully.");
                return true;
            }
            else
            {
                Logger.Error($"MSI execution failed with exit code {exitCode}.");
                return false;
            }
        }
    }
}
