namespace Curupira.Plugins.Contract
{
    public interface IPluginConfigParser<out T> where T : class
    {
        T Execute();
    }
}
