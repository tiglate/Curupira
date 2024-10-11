namespace Curupira.WindowsService.Wrappers
{
    public class EventLogWrapperFactory : IEventLogWrapperFactory
    {
        public IEventLogWrapper Create(string logName)
        {
            return new EventLogWrapper(logName);
        }
    }
}
