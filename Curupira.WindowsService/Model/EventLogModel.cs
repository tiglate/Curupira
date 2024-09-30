using System;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Model
{
    [ExcludeFromCodeCoverage]
    public class EventLogModel
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string EntryType { get; set; }
        public DateTime TimeGenerated { get; set; }
    }
}
