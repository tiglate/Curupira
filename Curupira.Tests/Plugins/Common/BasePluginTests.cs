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
}
