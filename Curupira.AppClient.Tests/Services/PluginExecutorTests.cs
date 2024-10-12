using Autofac;
using Curupira.AppClient;
using Curupira.AppClient.Services;
using Curupira.Plugins.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.AppClient.Tests.Services
{
    [TestClass]
    public class PluginExecutorTests
    {
        private IContainer _container;
        private Mock<ILogProvider> _loggerMock;
        private Mock<IProgressBarService> _progressBarServiceMock;
        private Mock<IConsoleService> _consoleServiceMock;
        private Mock<IPlugin> _pluginMock;

        private PluginExecutorTestable _pluginExecutor;

        public class PluginExecutorTestable : PluginExecutor
        {
            public PluginExecutorTestable(ILifetimeScope scope, ILogProvider logger, IProgressBarService progressBarService, IConsoleService consoleService)
                : base(scope, logger, progressBarService, consoleService) { }

            public new void AttachProgressHandler(IPlugin plugin, bool noProgressBar) => base.AttachProgressHandler(plugin, noProgressBar);
            public new bool TryInitializePlugin(IPlugin plugin, string pluginName) => base.TryInitializePlugin(plugin, pluginName);
            public new Task<bool> TryExecutePluginAsync(IPlugin plugin, Options options, CancellationToken cancellationToken) => base.TryExecutePluginAsync(plugin, options, cancellationToken);
            public new IDictionary<string, string> ParseParams(string paramString) => base.ParseParams(paramString);
        }

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogProvider>();
            _progressBarServiceMock = new Mock<IProgressBarService>();
            _consoleServiceMock = new Mock<IConsoleService>();
            _pluginMock = new Mock<IPlugin>();

            var builder = new ContainerBuilder();

            // Register mocked dependencies
            builder.RegisterInstance(_loggerMock.Object).As<ILogProvider>();
            builder.RegisterInstance(_progressBarServiceMock.Object).As<IProgressBarService>();
            builder.RegisterInstance(_consoleServiceMock.Object).As<IConsoleService>();

            // Register a mock plugin with a named registration for testing
            builder.RegisterInstance(_pluginMock.Object).Named<IPlugin>("TestPlugin");

            _container = builder.Build();

            // Initialize the plugin executor with Autofac's real container
            _pluginExecutor = new PluginExecutorTestable(
                _container.Resolve<ILifetimeScope>(),
                _loggerMock.Object,
                _progressBarServiceMock.Object,
                _consoleServiceMock.Object
            );
        }

        [TestMethod]
        public async Task ExecutePluginAsync_ShouldReturnFalse_WhenPluginNotFound()
        {
            // Arrange
            var options = new Options(_consoleServiceMock.Object) { Plugin = "NonExistentPlugin" };

            // Act
            var result = await _pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
            _consoleServiceMock.Verify(c => c.WriteLine(It.Is<string>(msg => msg.Contains("Plugin 'NonExistentPlugin' not found!"))), Times.Once);
            _loggerMock.Verify(l => l.Fatal(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecutePluginAsync_ShouldReturnFalse_WhenPluginInitializationFails()
        {
            // Arrange
            var options = new Options(_consoleServiceMock.Object) { Plugin = "TestPlugin" };
            _pluginMock.Setup(p => p.Init()).Throws(new Exception("Initialization failed"));

            // Act
            var result = await _pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("Error when initializing the plugin 'TestPlugin'"))), Times.Once);
        }

        [TestMethod]
        public async Task ExecutePluginAsync_ShouldExecuteSuccessfully_WithProgressBar()
        {
            // Arrange
            var options = new Options(_consoleServiceMock.Object) { Plugin = "TestPlugin", NoProgressBar = false };
            _pluginMock.Setup(p => p.ExecuteAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);
            _progressBarServiceMock.Verify(p => p.Init(10000, "Loading"), Times.Once);
            _progressBarServiceMock.Verify(p => p.SetMessage(It.IsAny<string>()), Times.AtLeastOnce);
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("executed successfully"))), Times.Once);
        }

        [TestMethod]
        public async Task ExecutePluginAsync_ShouldReturnFalse_WhenPluginExecutionFails()
        {
            // Arrange
            var options = new Options(_consoleServiceMock.Object) { Plugin = "TestPlugin" };
            _pluginMock.Setup(p => p.ExecuteAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pluginExecutor.ExecutePluginAsync(options).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("plugin failed"))), Times.Once);
        }

        [TestMethod]
        public void AttachProgressHandler_ShouldAttachProgressLogger_WhenNoProgressBar()
        {
            // Arrange
            var plugin = _pluginMock.Object;

            // Act
            _pluginExecutor.AttachProgressHandler(plugin, true);

            // Simulate progress event by triggering the event manually
            var progressEventArgs = new PluginProgressEventArgs(50, "Halfway");
            _pluginMock.Raise(p => p.Progress += null, progressEventArgs);

            // Assert
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("50%"))), Times.Once);
        }

        [TestMethod]
        public void AttachProgressHandler_ShouldAttachProgressBar_WhenProgressBarEnabled()
        {
            // Arrange
            var plugin = _pluginMock.Object;

            // Act
            _pluginExecutor.AttachProgressHandler(plugin, false);

            // Simulate progress event by triggering the event manually
            var progressEventArgs = new PluginProgressEventArgs(50, "Halfway");
            _pluginMock.Raise(p => p.Progress += null, progressEventArgs);

            // Assert
            _progressBarServiceMock.Verify(p => p.SetMessage(It.IsAny<string>()), Times.AtLeastOnce);
            _progressBarServiceMock.Verify(p => p.ReportProgress(It.IsAny<float>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void ParseParams_ShouldReturnEmptyDictionary_WhenParamsAreNullOrEmpty()
        {
            // Arrange
            var paramString = string.Empty;

            // Act
            var result = _pluginExecutor.ParseParams(paramString);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseParams_ShouldParseValidParams()
        {
            // Arrange
            var paramString = "key1=value1 key2=value2";

            // Act
            var result = _pluginExecutor.ParseParams(paramString);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"]);
            Assert.AreEqual("value2", result["key2"]);
        }

        [TestMethod]
        public void ParseParams_ShouldThrowFormatException_WhenInvalidParamFormat()
        {
            // Arrange
            var paramString = "key1=value1 invalidparam";

            // Act
            Assert.ThrowsException<FormatException>(() => _pluginExecutor.ParseParams(paramString));
        }
    }
}
