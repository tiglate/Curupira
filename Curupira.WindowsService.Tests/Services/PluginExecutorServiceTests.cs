using Autofac;
using Curupira.Plugins.Contract;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.WindowsService.Tests.Services
{
    [TestClass]
    public class PluginExecutorServiceTests
    {
        private IContainer _container;
        private Mock<ILogProvider> _mockLogger;
        private PluginExecutorService _service;

        [TestInitialize]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            _mockLogger = new Mock<ILogProvider>();

            // Register the mock logger
            builder.RegisterInstance(_mockLogger.Object).As<ILogProvider>();

            // Build the container
            _container = builder.Build();

            _service = new PluginExecutorService(_container, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ExecutePluginAsync_PluginNotRegistered_ReturnsFalse()
        {
            // Arrange
            var pluginName = "NonExistentPlugin";
            var pluginParams = new Dictionary<string, string>();

            // Act
            var result = await _service.ExecutePluginAsync(pluginName, pluginParams).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Verify(l => l.Error($"Plugin '{pluginName}' not found!"), Times.Once);
        }

        [TestMethod]
        public async Task ExecutePluginAsync_PluginExecutionFails_ReturnsFalse()
        {
            // Arrange
            var pluginName = "TestPlugin";
            var pluginParams = new Dictionary<string, string>();
            var mockPlugin = new Mock<IPlugin>();

            // Create a child lifetime scope and register the mock plugin
            using (var scope = _container.BeginLifetimeScope(builder =>
            {
                builder.RegisterInstance(mockPlugin.Object).Named<IPlugin>(pluginName);
            }))
            {
                var service = new PluginExecutorService(scope, _mockLogger.Object);

                mockPlugin.Setup(p => p.ExecuteAsync(pluginParams, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Execution failed"));

                // Act
                var result = await service.ExecutePluginAsync(pluginName, pluginParams).ConfigureAwait(false);

                // Assert
                Assert.IsFalse(result);
                _mockLogger.Verify(l => l.Error(It.IsAny<Exception>(), $"Error when executing the plugin '{pluginName}'"), Times.Once);
            }
        }

        [TestMethod]
        public async Task ExecutePluginAsync_PluginExecutesSuccessfully_ReturnsTrue()
        {
            // Arrange
            var pluginName = "TestPlugin";
            var pluginParams = new Dictionary<string, string>();
            var mockPlugin = new Mock<IPlugin>();

            // Create a child lifetime scope and register the mock plugin
            using (var scope = _container.BeginLifetimeScope(builder =>
            {
                builder.RegisterInstance(mockPlugin.Object).Named<IPlugin>(pluginName);
            }))
            {
                var service = new PluginExecutorService(scope, _mockLogger.Object);

                mockPlugin.Setup(p => p.ExecuteAsync(pluginParams, It.IsAny<CancellationToken>())).ReturnsAsync(true);

                // Act
                var result = await service.ExecutePluginAsync(pluginName, pluginParams).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result);
            }
        }
    }
}
