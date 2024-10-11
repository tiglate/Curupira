using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Curupira.Tests.WindowsService.Services
{
    [TestClass]
    public class SystemUsageServiceTests
    {
        private SystemUsageService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new SystemUsageService();
        }

        [TestMethod]
        public void GetSystemUsage_ReturnsValidSystemUsage()
        {
            // Act
            var result = _service.GetSystemUsage();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProcessorUsage >= 0 && result.ProcessorUsage <= 100);
            Assert.IsTrue(result.MemoryUsage >= 0 && result.MemoryUsage <= 100);
            Assert.IsNotNull(result.Disks);
            Assert.IsTrue(result.Disks.Count > 0);
            Assert.IsFalse(string.IsNullOrEmpty(result.WindowsStartTime));
        }

        [TestMethod]
        public void GetCpuUsage_ReturnsValidCpuUsage()
        {
            // Act
            var cpuUsage = typeof(SystemUsageService)
                .GetMethod("GetCpuUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);

            // Assert
            Assert.IsNotNull(cpuUsage);
            Assert.IsInstanceOfType(cpuUsage, typeof(double));
            Assert.IsTrue((double)cpuUsage >= 0 && (double)cpuUsage <= 100);
        }

        [TestMethod]
        public void GetMemoryUsage_ReturnsValidMemoryUsage()
        {
            // Act
            var memoryUsage = typeof(SystemUsageService)
                .GetMethod("GetMemoryUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);

            // Assert
            Assert.IsNotNull(memoryUsage);
            Assert.IsInstanceOfType(memoryUsage, typeof(double));
            Assert.IsTrue((double)memoryUsage >= 0 && (double)memoryUsage <= 100);
        }

        [TestMethod]
        public void GetDiskUsage_ReturnsValidDiskUsage()
        {
            // Act
            var diskUsage = typeof(SystemUsageService)
                .GetMethod("GetDiskUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null) as List<DiskUsageModel>;

            // Assert
            Assert.IsNotNull(diskUsage);
            Assert.IsTrue(diskUsage.Count > 0);
            foreach (var disk in diskUsage)
            {
                Assert.IsFalse(string.IsNullOrEmpty(disk.Name));
                Assert.IsTrue(disk.TotalSpace > 0);
                Assert.IsTrue(disk.FreeSpace >= 0);
                Assert.IsTrue(disk.FreeSpace <= disk.TotalSpace);
            }
        }

        [TestMethod]
        public void GetWindowsStartTime_ReturnsValidStartTime()
        {
            // Act
            var startTime = typeof(SystemUsageService)
                .GetMethod("GetWindowsStartTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null) as string;

            // Assert
            Assert.IsNotNull(startTime);
            Assert.IsFalse(string.IsNullOrEmpty(startTime));
        }
    }
}
