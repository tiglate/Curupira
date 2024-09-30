using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Model
{
    [ExcludeFromCodeCoverage]
    public class DiskUsageModel
    {
        public string Name { get; set; }
        public long TotalSpace { get; set; }
        public long FreeSpace { get; set; }
        public long UsedSpace => TotalSpace - FreeSpace;
    }
}
