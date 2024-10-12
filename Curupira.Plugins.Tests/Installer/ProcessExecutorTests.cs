using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Curupira.Plugins.Installer;

namespace Curupira.Plugins.Tests.Installer
{
    [TestClass]
    public class ProcessExecutorTests
    {
        private IProcessExecutor _processExecutor;

        [TestInitialize]
        public void SetUp()
        {
            _processExecutor = new ProcessExecutor();
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldReturnZeroExitCode_WhenCommandSucceeds()
        {
            // Arrange
            string fileName = "cmd.exe";
            string arguments = "/c echo Hello, World!";
            string workingDirectory = ".";

            // Act
            int exitCode = await _processExecutor.ExecuteAsync(fileName, arguments, workingDirectory);

            // Assert
            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldReturnNonZeroExitCode_WhenCommandFails()
        {
            // Arrange
            string fileName = "cmd.exe";
            string arguments = "/c invalidcommand";
            string workingDirectory = ".";

            // Act
            int exitCode = await _processExecutor.ExecuteAsync(fileName, arguments, workingDirectory);

            // Assert
            Assert.AreNotEqual(0, exitCode);
        }
    }
}
