using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curupira.WindowsService.Services
{
    public interface IPluginExecutorService
    {
        Task<bool> ExecutePluginAsync(string pluginName, IDictionary<string, string> pluginParams);
    }
}
