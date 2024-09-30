using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
    public class ProcessManager : IProcessManager
    {
        public void Kill(int processId)
        {
            using (var process = Process.GetProcessById(processId))
            {
                process.Kill();
            }
        }
    }
}
