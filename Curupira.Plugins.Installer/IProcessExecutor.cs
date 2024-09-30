using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public interface IProcessExecutor
    {
        Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory);
    }
}
