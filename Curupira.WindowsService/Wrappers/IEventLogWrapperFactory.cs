namespace Curupira.WindowsService.Wrappers
{
    public interface IEventLogWrapperFactory
    {
        IEventLogWrapper Create(string logName);
    }
}
