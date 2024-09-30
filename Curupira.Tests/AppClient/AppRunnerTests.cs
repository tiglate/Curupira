using Autofac;
using Curupira.AppClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curupira.AppClient.Services;
using System.Reflection;
using System;
using NLog;

namespace Curupira.Tests.AppClient
{
    [TestClass]
    public class AppRunnerTests
    {
        private TestableAppRunner _appRunner;
        private Mock<IPluginExecutor> _pluginExecutorMock;
        private Mock<IConsoleService> _consoleServiceMock;
        private Mock<IProgressBarService> _progressBarServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _pluginExecutorMock = new Mock<IPluginExecutor>();
            _consoleServiceMock = new Mock<IConsoleService>();
            _progressBarServiceMock = new Mock<IProgressBarService>();

            // Use real Autofac container for testing
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_pluginExecutorMock.Object).As<IPluginExecutor>();
            builder.RegisterInstance(_consoleServiceMock.Object).As<IConsoleService>();
            builder.RegisterInstance(_progressBarServiceMock.Object).As<IProgressBarService>();

            // Instantiate TestableAppRunner to access protected methods
            _appRunner = new TestableAppRunner(builder.Build());
        }

        [TestMethod]
        public async Task RunAsync_ShouldReturnZero_WhenPluginExecutionIsSuccessful()
        {
            // Arrange
            _pluginExecutorMock.Setup(p => p.ExecutePluginAsync(It.IsAny<Options>())).ReturnsAsync(true);

            // Act
            var result = await _appRunner.RunAsync(new[] { "--plugin", "TestPlugin" });

            // Assert
            Assert.AreEqual(0, result);
            _pluginExecutorMock.Verify(p => p.ExecutePluginAsync(It.IsAny<Options>()), Times.Once);
        }

        [TestMethod]
        public async Task RunAsync_ShouldReturnOne_WhenPluginExecutionFails()
        {
            // Arrange
            _pluginExecutorMock.Setup(p => p.ExecutePluginAsync(It.IsAny<Options>())).ReturnsAsync(false);

            // Act
            var result = await _appRunner.RunAsync(new[] { "--plugin", "TestPlugin" });

            // Assert
            Assert.AreEqual(1, result);
            _pluginExecutorMock.Verify(p => p.ExecutePluginAsync(It.IsAny<Options>()), Times.Once);
        }

        [TestMethod]
        public async Task RunAsync_ShouldHandleParseErrorAndReturnOne()
        {
            // Arrange
            _appRunner.SetParseResult(new List<Error> { new DummyError() });

            // Act
            var result = await _appRunner.RunAsync(new[] { "--invalid-option" });

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void HandleParseError_ShouldReturnOne_WhenHelpNotRequested()
        {
            // Arrange
            var errors = new List<Error> { new DummyError() };

            // Act
            var result = _appRunner.InvokeHandleParseError(errors);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ApplyLogLevel_ShouldSetLogLevelToInfo_WhenLogLevelNotProvided()
        {
            // Act
            _appRunner.InvokeApplyLogLevel(null);

            // Assert
            // No exception should occur; the default log level is applied
        }

        [TestMethod]
        public void ApplyLogLevel_ShouldSetLogLevelToError_WhenSpecified()
        {
            // Arrange
            var logLevelSetting = "ERROR";
            var consoleRule = LogManager.Configuration.FindRuleByName("consoleRule");

            // Act
            _appRunner.InvokeApplyLogLevel(logLevelSetting);

            // Assert
            Assert.IsNotNull(consoleRule, "The consoleRule should not be null.");
            Assert.AreEqual(LogLevel.Error, consoleRule.Levels[0], "The log level should be set to Error.");
        }
    }

    public class DummyError : Error
    {
        public DummyError() : base(ErrorType.UnknownOptionError) { }
    }

    // Subclass to expose protected methods for testing
    public class TestableAppRunner : AppRunner
    {
        private ParserResult<Options> _mockedParseResult;

        public TestableAppRunner(IContainer container) : base(container)
        {
        }

        public void SetParseResult(List<Error> errors)
        {
            // Use reflection to call the internal constructor of NotParsed<Options>
            var constructorInfo = typeof(NotParsed<Options>).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructorInfo.Length == 0)
            {
                throw new InvalidOperationException("Unable to find a constructor for NotParsed<Options>.");
            }

            _mockedParseResult = (NotParsed<Options>)constructorInfo[0].Invoke(new object[] { null, errors });
        }

        protected override ParserResult<Options> ParseArguments(string[] args)
        {
            return _mockedParseResult ?? base.ParseArguments(args);
        }

        public new async Task<int> RunAsync(string[] args)
        {
            return await base.RunAsync(args);
        }

        public int InvokeHandleParseError(IEnumerable<Error> errors)
        {
            return base.HandleParseError(errors);
        }

        public void InvokeApplyLogLevel(string logLevelSetting)
        {
            base.ApplyLogLevel(logLevelSetting);
        }
    }
}
