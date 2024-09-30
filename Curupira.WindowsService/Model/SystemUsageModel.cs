using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Model
{
    [ExcludeFromCodeCoverage]
    public class SystemUsageModel
    {
        public double ProcessorUsage { get; set; }
        public double MemoryUsage { get; set; }
        public List<DiskUsageModel> Disks { get; set; }
        public string WindowsStartTime { get; set; }
    }
}
