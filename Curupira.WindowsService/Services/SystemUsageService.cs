using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Curupira.WindowsService.Model;

namespace Curupira.WindowsService.Services
{
    public class SystemUsageService : ISystemUsageService
    {
        public SystemUsageModel GetSystemUsage()
        {
            var usage = new SystemUsageModel
            {
                ProcessorUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                Disks = GetDiskUsage(),
                WindowsStartTime = GetWindowsStartTime()
            };

            return usage;
        }

        // Get current CPU usage percentage
        private static double GetCpuUsage()
        {
            using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                cpuCounter.NextValue(); // First call returns 0, so we need a second call after a brief delay
                System.Threading.Thread.Sleep(500);
                return Math.Round(cpuCounter.NextValue(), 2);
            }
        }

        // Get memory usage percentage
        private static double GetMemoryUsage()
        {
            using (PerformanceCounter memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use"))
            {
                return Math.Round(memCounter.NextValue(), 2);
            }
        }

        // Get disk usage for all physical drives
        private static List<DiskUsageModel> GetDiskUsage()
        {
            var disks = new List<DiskUsageModel>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    var disk = new DiskUsageModel
                    {
                        Name = drive.Name,
                        TotalSpace = drive.TotalSize,
                        FreeSpace = drive.AvailableFreeSpace
                    };
                    disks.Add(disk);
                }
            }
            return disks;
        }

        // Get the time when Windows started
        private static string GetWindowsStartTime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    var os = searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault();
                    if (os != null)
                    {
                        DateTime bootTime = ManagementDateTimeConverter.ToDateTime(os["LastBootUpTime"].ToString());
                        return bootTime.ToString("g"); // Returns the boot time in a readable format
                    }
                }
            }
            catch (Exception)
            {
                return "Unknown";
            }
            return "Unknown";
        }
    }
}
