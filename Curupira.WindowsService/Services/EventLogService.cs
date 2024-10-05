using System;
using System.Collections.Generic;
using System.Diagnostics;
using Curupira.Plugins.Contract;
using Curupira.WindowsService.Model;

namespace Curupira.WindowsService.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly ILogProvider _logger;

        public EventLogService(ILogProvider logger)
        {
            _logger = logger;    
        }

        public IEnumerable<EventLogModel> GetLatestApplicationLogs(int maxEntries = 100)
        {
            var eventLogs = new List<EventLogModel>();

            try
            {
                // Open the "Application" log
                using (var applicationLog = new EventLog("Application"))
                {
                    var totalEntries = applicationLog.Entries.Count;

                    // Start from the most recent entry and go backwards
                    for (var i = totalEntries - 1; i >= 0 && eventLogs.Count < maxEntries; i--)
                    {
                        var entry = applicationLog.Entries[i];

                        var log = new EventLogModel
                        {
                            Source = entry.Source,
                            Message = entry.Message,
                            EntryType = entry.EntryType.ToString(),
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
