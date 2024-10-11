using Curupira.WindowsService.Model;
using System;
using System.Diagnostics;

namespace Curupira.WindowsService.Wrappers
{
    public class EventLogWrapper : IEventLogWrapper
    {
        private readonly EventLog _eventLog;

        public EventLogWrapper(string logName)
        {
            _eventLog = new EventLog(logName);
        }

        public int GetEntryCount()
        {
            return _eventLog.Entries.Count;
        }

        public CustomEventLogEntry GetEntry(int index)
        {
            var entry = _eventLog.Entries[index];
            return new CustomEventLogEntry
            {
                Source = entry.Source,
                Message = entry.Message,
                EntryType = entry.EntryType.ToString(),
                TimeGenerated = entry.TimeGenerated
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventLog?.Dispose();
            }
        }
    }
}
