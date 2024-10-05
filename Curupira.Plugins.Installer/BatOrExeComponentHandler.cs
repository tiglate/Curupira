using Curupira.Plugins.Contract;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public class BatOrExeComponentHandler : BaseComponentHandler
    {
        private readonly IProcessExecutor _processExecutor;

        public BatOrExeComponentHandler(IProcessExecutor processExecutor, ILogProvider logger)
            : base(logger)
        {
            _processExecutor = processExecutor;
        }

        public override async Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token)
        {
            if (!component.Parameters.TryGetValue("SourceFile", out string sourceFile))
            {
                throw new InvalidOperationException("Missing or empty 'SourceFile' parameter for BAT/EXE component.");
            }

            var additionalParams = GetAdditionalParams(component);
            var command = $"\"{sourceFile}\" {additionalParams}";

            Logger.Info($"Executing command: {command}");

            OnProgress(new PluginProgressEventArgs(0, "Executing..."));

            var exitCode = await _processExecutor.ExecuteAsync(sourceFile, additionalParams, new FileInfo(sourceFile).Directory.FullName);

            OnProgress(new PluginProgressEventArgs(100, "Completed."));

            if (exitCode == 0)
            {
                Logger.Info($"Execution completed successfully.");
                return true;
            }
            else
            {
                Logger.Error($"Execution failed with exit code {exitCode}.");
                return false;
            }
        }
    }
}
