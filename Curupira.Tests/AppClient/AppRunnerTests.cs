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
using Curupira.Plugins.Contract;
using System.Configuration;

namespace Curupira.Tests.AppClient
{
    [TestClass]
    public class AppRunnerTests
    {
        private TestableAppRunner _appRunner;
        private Mock<IPluginExecutor> _pluginExecutorMock;
        private Mock<IConsoleService> _consoleServiceMock;
        private Mock<IProgressBarService> _progressBarServiceMock;
        private Mock<IAutofacHelper> _autofacHelperMock;

        [TestInitialize]
        public void Setup()
        {
            _pluginExecutorMock = new Mock<IPluginExecutor>();
            _consoleServiceMock = new Mock<IConsoleService>();
            _progressBarServiceMock = new Mock<IProgressBarService>();
            _autofacHelperMock = new Mock<IAutofacHelper>();

            // Use real Autofac container for testing
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_pluginExecutorMock.Object).As<IPluginExecutor>();
            builder.RegisterInstance(_consoleServiceMock.Object).As<IConsoleService>();
            builder.RegisterInstance(_progressBarServiceMock.Object).As<IProgressBarService>();
            builder.RegisterInstance(_autofacHelperMock.Object).As<IAutofacHelper>();

            // Instantiate TestableAppRunner to access protected methods
            _appRunner = new TestableAppRunner(builder.Build());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _appRunner?.Dispose();
        }

        [TestMethod]
        public void ShowBanner_ShouldPrintBanner()
        {
            // Act
            _appRunner.ShowBanner();

            // Assert
            _consoleServiceMock.Verify(c => c.Clear(), Times.Once);
            _consoleServiceMock.Verify(c => c.WriteCentered(It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task RunAsync_ShouldListAllPlugins_WhenListPluginsOptionIsTrue()
        {
            // Simulate some plugins returned from AutofacHelper
            var plugins = new List<(string Name, Type Type)>
            {
                ("PluginA", typeof(IPlugin)),
                ("PluginB", typeof(IPlugin)),
            };

            // Setup AutofacHelper to return the mock plugins
            _autofacHelperMock.Setup(a => a.GetNamedImplementationsOfInterface<IPlugin>())
                .Returns(plugins);

            // Act
            var result = await _appRunner.RunAsync(new string[] { "-a" });

            // Assert
            Assert.AreEqual(0, result);  // Should return 0 for success
            _consoleServiceMock.Verify(c => c.WriteLine("PluginA"), Times.Once);  // Ensure "PluginA" was written
            _consoleServiceMock.Verify(c => c.WriteLine("PluginB"), Times.Once);  // Ensure "PluginB" was written
            _consoleServiceMock.VerifyNoOtherCalls();  // Ensure no extra calls were made
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
            var expectedLogLevel = ConfigurationManager.AppSettings["LogLevel"];

            _appRunner.InvokeApplyLogLevel(null);

            var consoleRule = LogManager.Configuration.FindRuleByName("consoleRule");

            Assert.IsNotNull(consoleRule, "The consoleRule should not be null.");

            Assert.AreEqual(expectedLogLevel, consoleRule.Levels[0].Name, $"The log level should be set to {expectedLogLevel}.");
        }

        [TestMethod]
        [DataRow("OFF")]
        [DataRow("TRACE")]
        [DataRow("DEBUG")]
        [DataRow("INFO")]
        [DataRow("WARN")]
        [DataRow("ERROR")]
        [DataRow("FATAL")]
        [DataRow("INVALID")]  // Invalid case
        [DataRow(null)]       // Null case
        public void ApplyLogLevel_ShouldSetCorrectLogLevel(string logLevelSetting)
        {
            // Arrange
            LogLevel expectedLogLevel;

            switch (logLevelSetting)
            {
                case "OFF":
                    expectedLogLevel = LogLevel.Off;
                    break;
                case "TRACE":
                    expectedLogLevel = LogLevel.Trace;
                    break;
                case "DEBUG":
                    expectedLogLevel = LogLevel.Debug;
                    break;
                case "INFO":
                    expectedLogLevel = LogLevel.Info;
                    break;
                case "WARN":
                    expectedLogLevel = LogLevel.Warn;
                    break;
                case "ERROR":
                    expectedLogLevel = LogLevel.Error;
                    break;
                case "FATAL":
                    expectedLogLevel = LogLevel.Fatal;
                    break;
                default:
                    expectedLogLevel = LogLevel.Info;  // Default for invalid or null values
                    break;
            }

            // Act
            _appRunner.InvokeApplyLogLevel(logLevelSetting);

            var consoleRule = LogManager.Configuration.FindRuleByName("consoleRule");

            // Assert

            Assert.IsNotNull(consoleRule, "The consoleRule should not be null.");

            if (expectedLogLevel == LogLevel.Off)
            {
                Assert.IsTrue(consoleRule.Levels.Count == 0, "If the log level = OFF, not levels should be found.");
            }
            else
            {
                Assert.AreEqual(expectedLogLevel, consoleRule.Levels[0], $"The log level should be set to {expectedLogLevel}.");
                Assert.AreEqual(LogLevel.Fatal, consoleRule.Levels[consoleRule.Levels.Count - 1], "The upper log level should be Fatal.");
            }
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

        public new void ShowBanner() => base.ShowBanner();

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
