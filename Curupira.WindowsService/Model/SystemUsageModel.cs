using System.Collections.Generic;

namespace Curupira.WindowsService.Model
{
    public class SystemUsageModel
    {
        public double ProcessorUsage { get; set; }
        public double MemoryUsage { get; set; }
        public List<DiskUsageModel> Disks { get; set; }
        public string WindowsStartTime { get; set; }
    }
}
