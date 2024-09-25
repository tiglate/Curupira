using Curupira.WindowsService.Model;
using System.Collections.Generic;

namespace Curupira.WindowsService.Service
{
    public interface IEventLogService
    {
        IEnumerable<EventLogModel> GetLatestApplicationLogs(int maxEntries = 100);
    }
}
