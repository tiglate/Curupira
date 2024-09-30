using System.Diagnostics;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public class ProcessExecutor : IProcessExecutor
    {
        public async Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    WorkingDirectory = workingDirectory,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                process.Start();

                await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync()).ConfigureAwait(false);
                process.WaitForExit();

                return process.ExitCode;
            }
        }
    }
}
