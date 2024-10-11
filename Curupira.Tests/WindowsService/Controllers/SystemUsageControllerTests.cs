using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Curupira.Tests.WindowsService.Controllers
{
    [TestClass]
    public class SystemUsageControllerTests
    {
        private Mock<ISystemUsageService> _mockSystemUsageService;
        private SystemUsageController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockSystemUsageService = new Mock<ISystemUsageService>();
            _controller = new SystemUsageController(_mockSystemUsageService.Object);
        }

        [TestMethod]
        public void GetSystemUsage_ReturnsSystemUsage()
        {
            // Arrange
            var expectedSystemUsage = new SystemUsageModel
            {
                ProcessorUsage = 25.5,
                MemoryUsage = 60.3,
                Disks = new List<DiskUsageModel>
                {
                    new DiskUsageModel { Name = "C:", TotalSpace = 500000, FreeSpace = 200000 },
                    new DiskUsageModel { Name = "D:", TotalSpace = 1000000, FreeSpace = 800000 }
                },
                WindowsStartTime = "2023-01-01T00:00:00"
            };
            _mockSystemUsageService.Setup(service => service.GetSystemUsage()).Returns(expectedSystemUsage);

            // Act
            var result = _controller.GetSystemUsage();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedSystemUsage.ProcessorUsage, result.ProcessorUsage);
            Assert.AreEqual(expectedSystemUsage.MemoryUsage, result.MemoryUsage);
            Assert.AreEqual(expectedSystemUsage.Disks.Count, result.Disks.Count);
            Assert.AreEqual(expectedSystemUsage.WindowsStartTime, result.WindowsStartTime);
        }
    }
}
