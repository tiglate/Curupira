using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Model
{
    [ExcludeFromCodeCoverage]
    public class TaskModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string LastRunTime { get; set; }
        public string NextRunTime { get; set; }
    }
}