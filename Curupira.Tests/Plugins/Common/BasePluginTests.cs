using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Curupira.Plugins.Common;
using Curupira.Plugins.Contract;

namespace Curupira.Tests.Plugins.Common
{
    [TestClass]
    public class BasePluginTests
    {
        private Mock<ILogProvider> _loggerMock;
        private Mock<IPluginConfigParser<object>> _configParserMock;
        private TestPlugin _testPlugin;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogProvider>();
            _configParserMock = new Mock<IPluginConfigParser<object>>();
            _testPlugin = new TestPlugin("TestPlugin", _loggerMock.Object, _configParserMock.Object);
        }

        [TestMethod]
        public void Init_ShouldSetConfigByCallingConfigParser()
        {
            // Arrange
            var expectedConfig = new object();
            _configParserMock.Setup(p => p.Execute()).Returns(expectedConfig);

            // Act
            _testPlugin.Init();

            // Assert
            Assert.AreEqual(expectedConfig, _testPlugin.Config, "Config should be set by calling the config parser.");
            _loggerMock.Verify(l => l.TraceMethod(nameof(BasePlugin<object>), nameof(_testPlugin.Init)), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldReturnTrue_WhenExecuteIsSuccessful()
        {
            // Arrange
            _testPlugin.SetExecuteResult(true);

            var commandLineArgs = new Dictionary<string, string>();

            // Act
            var result = await _testPlugin.ExecuteAsync(commandLineArgs);

            // Assert
            Assert.IsTrue(result, "ExecuteAsync should return true when Execute is successful.");
            _loggerMock.Verify(l => l.TraceMethod(nameof(BasePlugin<object>), nameof(_testPlugin.ExecuteAsync), nameof(commandLineArgs), commandLineArgs), Times.Once);
            _loggerMock.Verify(l => l.Debug(It.Is<string>(msg => msg.Contains("Executing plugin logic."))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldReturnFalse_WhenExecuteThrowsException()
        {
            // Arrange
            _testPlugin.SetThrowOnExecute(true);

            var commandLineArgs = new Dictionary<string, string>();

            // Act
            var result = await _testPlugin.ExecuteAsync(commandLineArgs);

            // Assert
            Assert.IsFalse(result, "ExecuteAsync should return false when Execute throws an exception.");
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("An error occurred during plugin execution."))), Times.Once);
        }

        [TestMethod]
        public async Task KillAsync_ShouldReturnTrue_WhenKillIsSuccessful()
        {
            // Arrange
            _testPlugin.SetKillResult(true);

            // Act
            var result = await _testPlugin.KillAsync();

            // Assert
            Assert.IsTrue(result, "KillAsync should return true when Kill is successful.");
            _loggerMock.Verify(l => l.TraceMethod(nameof(BasePlugin<object>), nameof(_testPlugin.KillAsync)), Times.Once);
            _loggerMock.Verify(l => l.Debug(It.Is<string>(msg => msg.Contains("Attempting to kill plugin."))), Times.Once);
        }

        [TestMethod]
        public async Task KillAsync_ShouldReturnFalse_WhenKillThrowsException()
        {
            // Arrange
            _testPlugin.SetThrowOnKill(true);

            // Act
            var result = await _testPlugin.KillAsync();

            // Assert
            Assert.IsFalse(result, "KillAsync should return false when Kill throws an exception.");
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("An error occurred during plugin kill."))), Times.Once);
        }

        [TestMethod]
        public void OnProgress_ShouldInvokeProgressEvent_AndLogProgress()
        {
            // Arrange
            var progressEventRaised = false;
            _testPlugin.Progress += (sender, e) =>
            {
                progressEventRaised = true;
                Assert.AreEqual(50, e.Percentage, "Progress percentage should be 50.");
                Assert.AreEqual("Halfway there", e.Message, "Progress message should be 'Halfway there'.");
            };

            var progressEventArgs = new PluginProgressEventArgs(50, "Halfway there");

            // Act
            _testPlugin.RaiseProgress(progressEventArgs);

            // Assert
            Assert.IsTrue(progressEventRaised, "Progress event should be raised.");
            _loggerMock.Verify(l => l.Debug(It.Is<string>(msg => msg.Contains("Progress: 50% - Halfway there"))), Times.Once);
        }

        [TestMethod]
        public void FormatLogMessage_ShouldIncludeTimestamp_WhenRequested()
        {
            // Act
            var resultWithTimestamp = _testPlugin.FormatTestLogMessage("TestMethod", "This is a test.", includeTimestamp: true);
            var resultWithoutTimestamp = _testPlugin.FormatTestLogMessage("TestMethod", "This is a test.", includeTimestamp: false);

            // Assert
            StringAssert.StartsWith(resultWithTimestamp, "[", "Formatted message with timestamp should start with a timestamp.");
            StringAssert.EndsWith(resultWithoutTimestamp, "This is a test.", "Formatted message without timestamp should not include a timestamp.");
        }
    }

    // Helper class to test the BasePlugin
    public class TestPlugin : BasePlugin<object>
    {
        private bool _executeResult = true;
        private bool _killResult = true;
        private bool _throwOnExecute = false;
        private bool _throwOnKill = false;

        public TestPlugin(string pluginName, ILogProvider logger, IPluginConfigParser<object> configParser)
            : base(pluginName, logger, configParser) { }

        public void SetExecuteResult(bool result) => _executeResult = result;
        public void SetKillResult(bool result) => _killResult = result;
        public void SetThrowOnExecute(bool shouldThrow) => _throwOnExecute = shouldThrow;
        public void SetThrowOnKill(bool shouldThrow) => _throwOnKill = shouldThrow;

        public override bool Execute(IDictionary<string, string> commandLineArgs)
        {
            if (_throwOnExecute)
                throw new InvalidOperationException("Simulated exception in Execute.");
            return _executeResult;
        }

        public override bool Kill()
        {
            if (_throwOnKill)
                throw new InvalidOperationException("Simulated exception in Kill.");
            return _killResult;
        }

        public void RaiseProgress(PluginProgressEventArgs e)
        {
            OnProgress(e);
        }

        public string FormatTestLogMessage(string method, string message, bool includeTimestamp = false)
        {
            return FormatLogMessage(method, message, includeTimestamp);
        }

        public override void Dispose()
        {
            // Dispose logic
        }
    }
}
