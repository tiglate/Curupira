namespace Curupira.Plugins.Contract
{
    public interface IPluginConfigParser<T> where T : class
    {
        T Execute();
    }
}
