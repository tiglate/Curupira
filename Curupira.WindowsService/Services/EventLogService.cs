using System;
using System.Collections.Generic;
using Curupira.Plugins.Contract;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Wrappers;

namespace Curupira.WindowsService.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly ILogProvider _logger;
        private readonly IEventLogWrapperFactory _eventLogWrapperFactory;

        public EventLogService(ILogProvider logger, IEventLogWrapperFactory eventLogWrapperFactory)
        {
            _logger = logger;
            _eventLogWrapperFactory = eventLogWrapperFactory;
        }

        public IEnumerable<EventLogModel> GetLatestApplicationLogs(int maxEntries = 100)
        {
            var eventLogs = new List<EventLogModel>();

            try
            {
                using (var applicationLog = _eventLogWrapperFactory.Create("Application"))
                {
                    var totalEntries = applicationLog.GetEntryCount();

                    for (var i = totalEntries - 1; i >= 0 && eventLogs.Count < maxEntries; i--)
                    {
                        var entry = applicationLog.GetEntry(i);

                        var log = new EventLogModel
                        {
                            Source = entry.Source,
                            Message = entry.Message,
                            EntryType = entry.EntryType,
                            TimeGenerated = entry.TimeGenerated
                        };

                        eventLogs.Add(log);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error retrieving event logs: {ex.Message}");
            }

            return eventLogs;
        }
    }
}
