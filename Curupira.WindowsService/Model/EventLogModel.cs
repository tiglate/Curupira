using System;

namespace Curupira.WindowsService.Model
{
    public class EventLogModel
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string EntryType { get; set; }
        public DateTime TimeGenerated { get; set; }
    }
}
