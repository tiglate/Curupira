using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
    public class MsServiceControllerFactory : IServiceControllerFactory
    {
        public IServiceController Build(string name)
        {
            return new MsServiceController(name);
        }

        public IServiceController Build(string name, string machineName)
        {
            return new MsServiceController(name, machineName);
        }
    }
}
