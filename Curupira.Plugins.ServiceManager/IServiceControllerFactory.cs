namespace Curupira.Plugins.ServiceManager
{
    public interface IServiceControllerFactory
    {
        IServiceController Build(string name);

        IServiceController Build(string name, string machineName);
    }
}
