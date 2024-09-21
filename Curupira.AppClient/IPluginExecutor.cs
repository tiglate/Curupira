using System.Threading.Tasks;

namespace Curupira.AppClient
{
    public interface IPluginExecutor
    {
        Task<bool> ExecutePluginAsync(Options options);
    }
}