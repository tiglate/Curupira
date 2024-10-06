using System.Threading;
using System.Threading.Tasks;

namespace Curupira.AppClient.Services
{
    public interface IPluginExecutor
    {
        Task<bool> ExecutePluginAsync(Options options, CancellationToken cancellationToken = default);
    }
}