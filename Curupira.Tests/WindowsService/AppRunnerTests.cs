using System;
using System.IO;
using Curupira.WindowsService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Curupira.Tests.WindowsService
{
    [TestClass]
    public class AppRunnerTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<TestableAppRunner> _mockAppRunner;

        [TestInitialize]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
            _mockAppRunner = new Mock<TestableAppRunner>(_mockLogger.Object) { CallBase = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockAppRunner.Object.Dispose();
            DeleteTempEnvFile();
        }

        [TestMethod]
        public void StartServer_ShouldLogStartMessages()
        {
            // Act
            _mockAppRunner.Object.StartServer();

            // Assert
            _mockLogger.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("OWIN server started"))), Times.Once);
        }

        [TestMethod]
        public void StopServer_ShouldLogStopMessages()
        {
            _mockAppRunner.Object.StartServer();

            // Act
            _mockAppRunner.Object.StopServer();

            // Assert
            _mockLogger.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("OWIN server stopped gracefully"))), Times.Once);
        }

        [TestMethod]
        public void SetEnvironmentVariablesInDevMode_ShouldSetVariables()
        {
            // Arrange
            string[] args = { "--api-key", "test-key" };

            // Act
            AppRunner.SetEnvironmentVariablesInDevMode(args);

            // Assert
            Assert.AreEqual("test-key", Environment.GetEnvironmentVariable("API_KEY"));
        }

        [TestMethod]
        public void SetEnvironmentVariablesInDevMode_ShouldUseEnvFile()
        {
            // Arrange
            var envContent = "API_KEY=env-file-key";
            WriteTempEnvFile(envContent);

            // Act
            AppRunner.SetEnvironmentVariablesInDevMode(new string[0]);

            // Assert
            Assert.AreEqual("env-file-key", Environment.GetEnvironmentVariable("API_KEY"));

            // Cleanup
            DeleteTempEnvFile();
        }

        private void WriteTempEnvFile(string content)
        {
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            File.WriteAllText(envFilePath, content);
        }

        private void DeleteTempEnvFile()
        {
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (File.Exists(envFilePath))
            {
                File.Delete(envFilePath);
            }
        }

        // Test-specific subclass to expose the protected StartOwin method
        public class TestableAppRunner : AppRunner
        {
            public TestableAppRunner(ILogger logger) : base(logger) { }

            protected override IDisposable StartOwin(string baseAddress)
            {
                return Mock.Of<IDisposable>();
            }
        }
    }
}
