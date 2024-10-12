using Curupira.Plugins.Contract;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Curupira.WindowsService.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curupira.WindowsService.Tests.Services
{
    [TestClass]
    public class EventLogServiceTests
    {
        private Mock<ILogProvider> _mockLogger;
        private Mock<IEventLogWrapper> _mockEventLogWrapper;
        private Mock<IEventLogWrapperFactory> _mockEventLogWrapperFactory;
        private EventLogService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogProvider>();
            _mockEventLogWrapper = new Mock<IEventLogWrapper>();
            _mockEventLogWrapperFactory = new Mock<IEventLogWrapperFactory>();
            _mockEventLogWrapperFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(_mockEventLogWrapper.Object);
            _service = new EventLogService(_mockLogger.Object, _mockEventLogWrapperFactory.Object);
        }

        [TestMethod]
        public void GetLatestApplicationLogs_ReturnsLogs()
        {
            // Arrange
            var eventLogEntries = new List<CustomEventLogEntry>
            {
                new CustomEventLogEntry
                {
                    Source = "TestSource",
                    Message = "TestMessage",
                    EntryType = "Information",
                    TimeGenerated = DateTime.Now
                }
            };

            _mockEventLogWrapper.Setup(log => log.GetEntryCount()).Returns(eventLogEntries.Count);
            _mockEventLogWrapper.Setup(log => log.GetEntry(It.IsAny<int>())).Returns((int index) => eventLogEntries[index]);

            // Act
            var result = _service.GetLatestApplicationLogs().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("TestSource", result[0].Source);
            Assert.AreEqual("TestMessage", result[0].Message);
            Assert.AreEqual("Information", result[0].EntryType);
        }

        [TestMethod]
        public void GetLatestApplicationLogs_HandlesException()
        {
            // Arrange
            var exceptionMessage = "Test exception";
            _mockEventLogWrapper.Setup(log => log.GetEntryCount()).Throws(new Exception(exceptionMessage));

            // Act
            var result = _service.GetLatestApplicationLogs().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains(exceptionMessage))), Times.Once);
        }

        [TestMethod]
        public void GetLatestApplicationLogs_ReturnsOnlyLatest10Logs()
        {
            // Arrange
            var eventLogEntries = new List<CustomEventLogEntry>();
            for (int i = 0; i < 100; i++)
            {
                eventLogEntries.Add(new CustomEventLogEntry
                {
                    Source = $"TestSource{i}",
                    Message = $"TestMessage{i}",
                    EntryType = "Information",
                    TimeGenerated = DateTime.Now.AddMinutes(-i)
                });
            }

            _mockEventLogWrapper.Setup(log => log.GetEntryCount()).Returns(eventLogEntries.Count);
            _mockEventLogWrapper.Setup(log => log.GetEntry(It.IsAny<int>())).Returns((int index) => eventLogEntries[index]);

            // Act
            var result = _service.GetLatestApplicationLogs(10).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual($"TestSource{100 - i - 1}", result[i].Source);
                Assert.AreEqual($"TestMessage{100 - i - 1}", result[i].Message);
                Assert.AreEqual("Information", result[i].EntryType);
            }
        }

        [TestMethod]
        public void GetLatestApplicationLogs_ReturnsLogsUsingTheActualEventLog()
        {
            // Arrange
            _service = new EventLogService(_mockLogger.Object, new EventLogWrapperFactory());
            // Act
            var result = _service.GetLatestApplicationLogs().ToList();
            // Assert
            Assert.IsNotNull(result);
        }
    }
}