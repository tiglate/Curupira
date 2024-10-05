using Curupira.Plugins.Contract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Plugins.Installer
{
    public interface IComponentHandler
    {
        event EventHandler<PluginProgressEventArgs> Progress;

        Task<bool> HandleAsync(Component component, bool ignoreUnauthorizedAccess, CancellationToken token);
    }
}
