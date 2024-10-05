using Curupira.WindowsService.Model;
using System.Collections.Generic;

namespace Curupira.WindowsService.Services
{
    public interface IEventLogService
    {
        IEnumerable<EventLogModel> GetLatestApplicationLogs(int maxEntries = 100);
    }
}
