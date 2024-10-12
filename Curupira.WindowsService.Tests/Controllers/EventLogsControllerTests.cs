using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curupira.WindowsService.Tests.Controllers
{
    [TestClass]
    public class EventLogsControllerTests
    {
        private Mock<IEventLogService> _mockEventLogService;
        private EventLogsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockEventLogService = new Mock<IEventLogService>();
            _controller = new EventLogsController(_mockEventLogService.Object);
        }

        [TestMethod]
        public void GetAll_ReturnsAllEventLogs()
        {
            // Arrange
            var expectedLogs = new List<EventLogModel>
            {
                new EventLogModel { Source = "Source1", Message = "Message1", EntryType = "Information", TimeGenerated = DateTime.UtcNow.AddMinutes(-10) },
                new EventLogModel { Source = "Source2", Message = "Message2", EntryType = "Error", TimeGenerated = DateTime.UtcNow.AddMinutes(-5) }
            };
            _mockEventLogService.Setup(service => service.GetLatestApplicationLogs(It.IsAny<int>())).Returns(expectedLogs);

            // Act
            var result = _controller.GetAll() as IEnumerable<EventLogModel>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLogs.Count, result.Count());
            CollectionAssert.AreEqual(expectedLogs, result.ToList());
        }
    }
}

