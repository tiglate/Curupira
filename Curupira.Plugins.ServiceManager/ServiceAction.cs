using System.Diagnostics.CodeAnalysis;

namespace Curupira.Plugins.ServiceManager
{
    [ExcludeFromCodeCoverage]
    public class ServiceAction
    {
        public string ServiceName { get; set; }

        public Action Action { get; set; }

        public ServiceAction(string serviceName, Action action)
        {
            ServiceName = serviceName;
            Action = action;
        }
    }
}
