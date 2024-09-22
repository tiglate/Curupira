namespace Curupira.Plugins.ServiceManager
{
    public class ServiceAction
    {
        public string ServiceName { get; set; }

        public ActionEnum Action { get; set; }

        public ServiceAction(string serviceName, ActionEnum action)
        {
            ServiceName = serviceName;
            Action = action;
        }
    }
}
