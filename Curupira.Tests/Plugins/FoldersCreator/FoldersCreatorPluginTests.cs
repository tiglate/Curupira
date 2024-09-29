using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Curupira.Plugins.FoldersCreator;
using Curupira.Plugins.Contract;
using System.Threading.Tasks;
using Moq.Protected;
using System.Security.AccessControl;
using System;

namespace Curupira.Tests.Plugins.FoldersCreator
{
    [TestClass]
    public class FoldersCreatorPluginTests
    {
        private Mock<ILogProvider> _loggerMock;
        private Mock<IPluginConfigParser<FoldersCreatorPluginConfig>> _configParserMock;
        private FoldersCreatorPlugin _foldersCreatorPlugin;
        private FoldersCreatorPluginConfig _pluginConfig;
        private List<string> _testDirectories;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogProvider>();
            _configParserMock = new Mock<IPluginConfigParser<FoldersCreatorPluginConfig>>();
            _pluginConfig = new FoldersCreatorPluginConfig();
            _testDirectories = new List<string>();

            _configParserMock.Setup(p => p.Execute()).Returns(_pluginConfig);

            _foldersCreatorPlugin = new FoldersCreatorPlugin(_loggerMock.Object, _configParserMock.Object);
            _foldersCreatorPlugin.Init();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete any test directories created during the tests
            foreach (var directory in _testDirectories)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            _foldersCreatorPlugin?.Dispose();
        }

        [TestMethod]
        public void Execute_ShouldCreateDirectories_WhenTheyDoNotExist()
        {
            // Arrange
            var directory1 = Path.Combine(Path.GetTempPath(), "tempDir1");
            var directory2 = Path.Combine(Path.GetTempPath(), "tempDir2");
            _pluginConfig.DirectoriesToCreate.Add(directory1);
            _pluginConfig.DirectoriesToCreate.Add(directory2);

            _testDirectories.Add(directory1);
            _testDirectories.Add(directory2);

            // Act
            var result = _foldersCreatorPlugin.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsTrue(result, "Plugin should execute successfully.");
            Assert.IsTrue(Directory.Exists(directory1), "Directory1 should be created.");
            Assert.IsTrue(Directory.Exists(directory2), "Directory2 should be created.");
            _loggerMock.Verify(l => l.TraceMethod("FoldersCreatorPlugin", "Execute", "commandLineArgs", It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldSkipExistingDirectories()
        {
            // Arrange
            var existingDirectory = Path.Combine(Path.GetTempPath(), "existingDir");
            Directory.CreateDirectory(existingDirectory);
            _pluginConfig.DirectoriesToCreate.Add(existingDirectory);

            _testDirectories.Add(existingDirectory);

            // Act
            var result = _foldersCreatorPlugin.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsTrue(result, "Plugin should execute successfully.");
            Assert.IsTrue(Directory.Exists(existingDirectory), "Existing directory should not be recreated.");
            _loggerMock.Verify(l => l.TraceMethod("FoldersCreatorPlugin", "Execute", "commandLineArgs", It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldLogError_WhenInvalidDirectoryIsProvided()
        {
            // Arrange
            var invalidDirectory = "Z:\\Invalid\\Path";
            _pluginConfig.DirectoriesToCreate.Add(invalidDirectory);

            // Act
            var result = _foldersCreatorPlugin.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsFalse(result, "Plugin should fail when provided with an invalid directory.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("Invalid path"))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldStopExecution_WhenKilled()
        {
            // Arrange
            var directory1 = Path.Combine(Path.GetTempPath(), "tempDir1");
            var directory2 = Path.Combine(Path.GetTempPath(), "tempDir2");
            _pluginConfig.DirectoriesToCreate.Add(directory1);
            _pluginConfig.DirectoriesToCreate.Add(directory2);

            _testDirectories.Add(directory1);
            _testDirectories.Add(directory2);

            // Simulate a delay during directory creation to mimic a long-running task
            var plugin = new FoldersCreatorPluginWithDelay(_loggerMock.Object, _configParserMock.Object);
            plugin.Init();

            // Act
            var executeTask = plugin.ExecuteAsync(new Dictionary<string, string>());

            // Wait for a short period and then call KillAsync to simulate stopping the task mid-execution
            await Task.Delay(500); // Wait a bit before killing the execution
            await plugin.KillAsync();

            // Wait for the execution to complete
            var result = await executeTask;

            // Assert
            Assert.IsFalse(result, "Execution should be cancelled when the plugin is killed.");
            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("Plugin execution cancelled"))), Times.Once);
        }

        [TestMethod]
        public void HasCreateDirectoryPermission_ShouldReturnTrue_WhenUserHasPermission()
        {
            // Arrange
            var tempDirectory = Path.GetTempPath(); // Use temp directory since most users will have access

            // Act
            var result = InvokeHasCreateDirectoryPermission(tempDirectory);

            // Assert
            Assert.IsTrue(result, "User should have permission to create directories in the temp path.");
        }

        [TestMethod]
        public void HasCreateDirectoryPermission_ShouldReturnFalse_WhenUserDoesNotHavePermission()
        {
            // Arrange
            var protectedDirectory = @"C:\Windows"; // Assume most users don't have write access to C:\Windows

            // Act
            var result = InvokeHasCreateDirectoryPermission(protectedDirectory);

            // Assert
            Assert.IsFalse(result, "User should not have permission to create directories in protected paths.");
        }

        [TestMethod]
        public void HasCreateDirectoryPermission_ShouldLogError_WhenUnauthorizedAccessExceptionIsThrown()
        {
            // Arrange
            var tempDirectory = Path.GetTempPath(); // Use a temp directory for testing

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.GetAccessControl to throw UnauthorizedAccessException
            pluginMock
                .Protected()
                .Setup<FileSystemSecurity>("GetAccessControl", ItExpr.IsAny<string>())
                .Throws(new UnauthorizedAccessException());

            // Act
            var result = InvokeHasCreateDirectoryPermission(tempDirectory, pluginMock.Object);

            // Assert
            Assert.IsFalse(result, "The method should return false when UnauthorizedAccessException is thrown.");

            // Correct the verification to match the actual call, with the message format and argument
            _loggerMock.Verify(l => l.Error("UnauthorizedAccessException: No permission to access the directory {0}", tempDirectory), Times.Once);
        }

        [TestMethod]
        public void HasCreateDirectoryPermission_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var tempDirectory = Path.GetTempPath(); // Use a temp directory for testing

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.GetAccessControl to throw a general Exception
            pluginMock
                .Protected()
                .Setup<FileSystemSecurity>("GetAccessControl", ItExpr.IsAny<string>())
                .Throws(new Exception("Test exception"));

            // Act
            var result = InvokeHasCreateDirectoryPermission(tempDirectory, pluginMock.Object);

            // Assert
            Assert.IsFalse(result, "The method should return false when a general Exception is thrown.");
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("An error occurred"))), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldCreateNetworkShareDirectory_WhenDirectoryPathIsNetworkPath()
        {
            // Arrange
            var networkPath = @"\\192.168.1.1\mytest\sharedfolder";
            _pluginConfig.DirectoriesToCreate.Add(networkPath);

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.Exists to return false (directory does not exist yet)
            pluginMock
                .Protected()
                .Setup<bool>("DirectoryExists", ItExpr.Is<string>(path => path == networkPath))
                .Returns(false);

            // Mock Directory.CreateDirectory to simulate creating a network share folder
            var directoryCreated = false;
            pluginMock
                .Protected()
                .Setup<DirectoryInfo>("CreateDirectory", ItExpr.Is<string>(path => path.StartsWith(@"\\")))
                .Callback(() => directoryCreated = true)
                .Returns(new DirectoryInfo(networkPath));

            pluginMock.Object.Init(); // Initialize the plugin

            // Act
            var result = pluginMock.Object.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsTrue(result, "The plugin should successfully create the network share directory.");
            Assert.IsTrue(directoryCreated, "The network share directory should be created.");
            _loggerMock.Verify(l => l.Debug(It.Is<string>(msg => msg.Contains("Processed 1 of 1 directories"))), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldLogError_WhenIOExceptionIsThrownDuringNetworkDirectoryCreation()
        {
            // Arrange
            var networkPath = @"\\192.168.1.1\mytest\sharedfolder";
            _pluginConfig.DirectoriesToCreate.Add(networkPath);

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.CreateDirectory to throw IOException
            pluginMock
                .Protected()
                .Setup<DirectoryInfo>("CreateDirectory", ItExpr.Is<string>(path => path.StartsWith(@"\\")))
                .Throws(new IOException());

            pluginMock.Object.Init(); // Initialize the plugin

            // Act
            var result = pluginMock.Object.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsFalse(result, "The plugin should return false when IOException is thrown.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("An error occurred during network directory creation"))), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldLogError_WhenUnauthorizedAccessExceptionIsThrownDuringNetworkDirectoryCreation()
        {
            // Arrange
            var networkPath = @"\\192.168.1.1\mytest\sharedfolder";
            _pluginConfig.DirectoriesToCreate.Add(networkPath);

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.CreateDirectory to throw UnauthorizedAccessException
            pluginMock
                .Protected()
                .Setup<DirectoryInfo>("CreateDirectory", ItExpr.Is<string>(path => path.StartsWith(@"\\")))
                .Throws(new UnauthorizedAccessException());

            pluginMock.Object.Init(); // Initialize the plugin

            // Act
            var result = pluginMock.Object.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsFalse(result, "The plugin should return false when UnauthorizedAccessException is thrown.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("Insufficient permissions to create directory"))), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldLogError_WhenNoPermissionsToCreateDirectory()
        {
            // Arrange
            var directoryPath = Path.Combine(Path.GetTempPath(), "tempDirNoPermissions");
            _pluginConfig.DirectoriesToCreate.Add(directoryPath);

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock HasCreateDirectoryPermission to return false, simulating insufficient permissions
            pluginMock
                .Protected()
                .Setup<bool>("HasCreateDirectoryPermission", ItExpr.IsAny<string>())
                .Returns(false);

            pluginMock.Object.Init(); // Initialize the plugin

            // Act
            var result = pluginMock.Object.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsFalse(result, "The plugin should return false when it doesn't have permission to create a directory.");
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains($"Insufficient permissions to create directory '{directoryPath}'"))), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldLogError_WhenExceptionIsThrownDuringDirectoryCreation()
        {
            // Arrange
            var directoryPath = Path.Combine(Path.GetTempPath(), "tempDirException");
            _pluginConfig.DirectoriesToCreate.Add(directoryPath);

            var pluginMock = new Mock<FoldersCreatorPlugin>(_loggerMock.Object, _configParserMock.Object) { CallBase = true };

            // Mock Directory.CreateDirectory to throw a general Exception
            pluginMock
                .Protected()
                .Setup<DirectoryInfo>("CreateDirectory", ItExpr.IsAny<string>())
                .Throws(new Exception("Test exception during directory creation"));

            pluginMock.Object.Init(); // Initialize the plugin

            // Act
            var result = pluginMock.Object.Execute(new Dictionary<string, string>());

            // Assert
            Assert.IsFalse(result, "The plugin should return false when an exception is thrown during directory creation.");
            _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("An error occurred during directory creation"))), Times.Once);
        }

        private bool InvokeHasCreateDirectoryPermission(string path)
        {
            var method = typeof(FoldersCreatorPlugin).GetMethod("HasCreateDirectoryPermission", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(_foldersCreatorPlugin, new object[] { path });
        }

        private bool InvokeHasCreateDirectoryPermission(string path, FoldersCreatorPlugin plugin)
        {
            var method = typeof(FoldersCreatorPlugin).GetMethod("HasCreateDirectoryPermission", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(plugin, new object[] { path });
        }

    }
}
