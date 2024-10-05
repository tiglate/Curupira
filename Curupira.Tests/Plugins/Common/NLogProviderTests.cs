using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;
using Curupira.Plugins.Common;
using Moq;

namespace Curupira.Tests.Plugins.Common
{
    [TestClass]
    public class NLogProviderTests
    {
        private MemoryTarget _memoryTarget;
        private NLogProvider _nlogProvider;

        [TestInitialize]
        public void Setup()
        {
            // Setup NLog memory target for capturing log messages
            _memoryTarget = new MemoryTarget { Layout = "${message}" };

            var config = new LoggingConfiguration();
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, _memoryTarget);
            LogManager.Configuration = config;

            // Initialize the NLogProvider
            _nlogProvider = new NLogProvider();
        }

        [TestMethod]
        public void Constructor_ShouldInitializeLogger()
        {
            // Arrange
            var mockLogger = new Mock<Logger>(); // Create a mock Logger

            // Act
            var nlogProvider = new NLogProvider(mockLogger.Object);

            // Assert
            Assert.IsNotNull(nlogProvider, "The NLogProvider instance should be created.");
            Assert.AreEqual(mockLogger.Object, nlogProvider.InnerLogger, "The logger should be correctly initialized.");
        }

        [TestMethod]
        public void Trace_ShouldLogTraceMessage()
        {
            // Arrange
            var message = "Trace message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Trace(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual("Trace message", logMessages[0]);
        }

        [TestMethod]
        public void Debug_ShouldLogDebugMessage()
        {
            // Arrange
            var message = "Debug message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Debug(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual("Debug message", logMessages[0]);
        }

        [TestMethod]
        public void Info_ShouldLogInfoMessage()
        {
            // Arrange
            var message = "Info message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Info(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual("Info message", logMessages[0]);
        }

        [TestMethod]
        public void Warn_ShouldLogWarnMessage()
        {
            // Arrange
            var message = "Warn message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Warn(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual("Warn message", logMessages[0]);
        }

        [TestMethod]
        public void Error_ShouldLogErrorMessageWithException()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var message = "Error message";

            // Act
            _nlogProvider.Error(exception, message);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.IsTrue(logMessages[0].Contains("Error message"));
        }

        [TestMethod]
        public void Error_ShouldLogErrorMessage()
        {
            // Arrange
            var message = "Error message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Error(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count, "An error message should be logged.");
            Assert.AreEqual("Error message", logMessages[0], "The error message should match the input.");
        }

        [TestMethod]
        public void Error_ShouldLogException_WhenMessageIsNull()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _nlogProvider.Error(exception, null);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count, "An error message should be logged with only the exception.");
            Assert.IsTrue(logMessages[0].Contains("Test exception"), "The logged message should contain the exception message.");
        }

        [TestMethod]
        public void Fatal_ShouldLogFatalMessage()
        {
            // Arrange
            var message = "Fatal message";
            var args = new object[] { "arg1", "arg2" };

            // Act
            _nlogProvider.Fatal(message, args);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count, "A fatal message should be logged.");
            Assert.AreEqual("Fatal message", logMessages[0], "The fatal message should match the input.");
        }

        [TestMethod]
        public void Fatal_ShouldLogFatalMessageWithException()
        {
            // Arrange
            var exception = new Exception("Test fatal exception");
            var message = "Fatal message";

            // Act
            _nlogProvider.Fatal(exception, message);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.IsTrue(logMessages[0].Contains("Fatal message"));
        }

        [TestMethod]
        public void Fatal_ShouldLogException_WhenMessageIsNull()
        {
            // Arrange
            var exception = new Exception("Test fatal exception");

            // Act
            _nlogProvider.Fatal(exception, null);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count, "A fatal message should be logged with only the exception.");
            Assert.IsTrue(logMessages[0].Contains("Test fatal exception"), "The logged message should contain the exception message.");
        }

        [TestMethod]
        public void TraceMethod_ShouldLogMethodEntry_WithParameters()
        {
            // Arrange
            var parameters = new object[] { "param1", "value1", "param2", 123 };
            var expectedLogMessage = "TestClass.TestMethod(param1 = \"value1\", param2 = 123)";

            // Act
            _nlogProvider.TraceMethod("TestClass", "TestMethod", parameters);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual(expectedLogMessage, logMessages[0]);
        }

        [TestMethod]
        public void TraceMethod_ShouldLogMethodEntry_WithDictionaryParameters()
        {
            // Arrange
            var dictionary = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var parameters = new object[] { "param1", dictionary };
            var expectedLogMessage = "TestClass.TestMethod(param1 = { \"key1\" => \"value1\", \"key2\" => \"value2\" })";

            // Act
            _nlogProvider.TraceMethod("TestClass", "TestMethod", parameters);

            // Assert
            var logMessages = _memoryTarget.Logs;
            Assert.AreEqual(1, logMessages.Count);
            Assert.AreEqual(expectedLogMessage, logMessages[0]);
        }
    }
}
