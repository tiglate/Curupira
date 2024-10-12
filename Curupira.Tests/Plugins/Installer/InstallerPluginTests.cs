using Curupira.Plugins.Contract;
using Curupira.Plugins.Installer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Curupira.Tests.Plugins.ServiceManager
{
    [TestClass]
    public class InstallerPluginTests
    {
        private Mock<ILogProvider> _loggerMock;
        private Mock<IPluginConfigParser<InstallerPluginConfig>> _configParserMock;
        private Mock<IProcessExecutor> _processExecutorMock;
        private InstallerPlugin _plugin;
        private InstallerPluginConfig _pluginConfig;

        [TestInitialize]
        public void Setup()
        {
            // Initialize the mocks
            _loggerMock = new Mock<ILogProvider>();
            _configParserMock = new Mock<IPluginConfigParser<InstallerPluginConfig>>();
            _processExecutorMock = new Mock<IProcessExecutor>();

            // Setup a basic InstallerPluginConfig for the tests
            _pluginConfig = new InstallerPluginConfig();
            _configParserMock.Setup(cp => cp.Execute()).Returns(_pluginConfig);

            // Initialize the plugin with the mocks
            _plugin = new InstallerPlugin(_loggerMock.Object, _configParserMock.Object, _processExecutorMock.Object);

            // Initialize the plugin (this will set the Config property)
            _plugin.Init();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _plugin?.Dispose();
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldExtractZipComponentSuccessfully_RealFileSystem()
        {
            // Arrange
            var component = new Component("TestZip", ComponentType.Zip, ComponentAction.None);
            var tempFolder = Path.Combine(Path.GetTempPath(), "InstallerPluginTests");
            Directory.CreateDirectory(tempFolder); // Ensure the directory exists

            var sourceZipPath = Path.Combine(tempFolder, "test.zip");
            var targetDir = Path.Combine(tempFolder, "output");

            component.Parameters["SourceFile"] = sourceZipPath;
            component.Parameters["TargetDir"] = targetDir;

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestZip" } };

            // Create a zip file dynamically
            using (var zipFileStream = new FileStream(sourceZipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    var zipEntry = archive.CreateEntry("test.txt");
                    using (var entryStream = zipEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        await writer.WriteAsync("This is a test file inside the zip.").ConfigureAwait(false);
                    }
                }
            }

            try
            {
                // Act
                var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "The plugin should have successfully extracted the ZIP component.");

                // Verify the extracted file exists in the target directory
                var extractedFilePath = Path.Combine(targetDir, "test.txt");
                Assert.IsTrue(File.Exists(extractedFilePath), $"The file {extractedFilePath} should have been extracted.");
            }
            finally
            {
                // Cleanup the test files
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true); // Clean up the entire test folder
                }
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldRunMsiComponentSuccessfully()
        {
            // Arrange
            var component = new Component("TestMsi", ComponentType.Msi, ComponentAction.Install);
            component.Parameters["SourceFile"] = "C:\\test.msi";
            component.Parameters["Params"] = "/qn";

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestMsi" } };

            // Mock the process executor to simulate successful MSI execution
            _processExecutorMock.Setup(p => p.ExecuteAsync("msiexec.exe", "/i \"C:\\test.msi\" /qn", It.IsAny<string>()))
                .ReturnsAsync(0);  // Exit code 0 means success

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should have successfully executed the MSI component.");
            _processExecutorMock.Verify(p => p.ExecuteAsync("msiexec.exe", "/i \"C:\\test.msi\" /qn", It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldRunBatComponentSuccessfully()
        {
            // Arrange
            var component = new Component("TestBat", ComponentType.Bat, ComponentAction.None);
            component.Parameters["SourceFile"] = "C:\\test.bat";
            component.Parameters["Params"] = "/S";

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestBat" } };

            // Mock the process executor to simulate successful BAT execution
            _processExecutorMock.Setup(p => p.ExecuteAsync("C:\\test.bat", "/S", It.IsAny<string>()))
                .ReturnsAsync(0);  // Exit code 0 means success

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result, "The plugin should have successfully executed the BAT component.");
            _processExecutorMock.Verify(p => p.ExecuteAsync("C:\\test.bat", "/S", It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogFatalAndReturnFalse_WhenComponentNotFound()
        {
            // Arrange
            var commandLineArgs = new Dictionary<string, string>
        {
            { "component", "non_existing_component" }  // Simulate a component that doesn't exist
        };

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the component is not found.");
            _loggerMock.Verify(l => l.Fatal(It.Is<string>(msg => msg.Contains("Component 'non_existing_component' not found."))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldSkipSpecifiedItems_RealFileSystem()
        {
            // Arrange
            var component = new Component("TestZip", ComponentType.Zip, ComponentAction.None);
            var tempFolder = Path.Combine(Path.GetTempPath(), "InstallerPluginSkipTests");
            Directory.CreateDirectory(tempFolder); // Ensure the directory exists

            var sourceZipPath = Path.Combine(tempFolder, "test.zip");
            var targetDir = Path.Combine(tempFolder, "output");

            component.Parameters["SourceFile"] = sourceZipPath;
            component.Parameters["TargetDir"] = targetDir;

            // Add items to be skipped
            component.RemoveItems.Add("*.ini");
            component.RemoveItems.Add(@"help\*.html");
            component.RemoveItems.Add("license.txt");
            component.RemoveItems.Add(@"**\*.logs");

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestZip" } };

            // Create a zip file dynamically with various files and folders
            using (var zipFileStream = new FileStream(sourceZipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    // Files that should be extracted
                    CreateZipEntry(archive, "app.exe", "This is the application executable.");
                    CreateZipEntry(archive, "config.dll", "This is a library file.");
                    CreateZipEntry(archive, "data.db", "This is the database file.");

                    // Files that should be skipped
                    CreateZipEntry(archive, "settings.ini", "This file should be skipped.");
                    CreateZipEntry(archive, @"help\manual.html", "This HTML file should be skipped.");
                    CreateZipEntry(archive, "license.txt", "This license file should be skipped.");
                    CreateZipEntry(archive, @"logs\app.logs", "Log files should be skipped.");
                }
            }

            try
            {
                // Act
                var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "The plugin should have successfully extracted the ZIP component, skipping the specified items.");

                // Verify that the files were extracted (excluding the ones that should be skipped)
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "app.exe")), "app.exe should have been extracted.");
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "config.dll")), "config.dll should have been extracted.");
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "data.db")), "data.db should have been extracted.");

                // Verify that the skipped files were not extracted
                Assert.IsFalse(File.Exists(Path.Combine(targetDir, "settings.ini")), "settings.ini should have been skipped.");
                Assert.IsFalse(File.Exists(Path.Combine(targetDir, @"help\manual.html")), "manual.html should have been skipped.");
                Assert.IsFalse(File.Exists(Path.Combine(targetDir, "license.txt")), "license.txt should have been skipped.");
                Assert.IsFalse(File.Exists(Path.Combine(targetDir, @"logs\app.logs")), "app.logs should have been skipped.");
            }
            finally
            {
                // Cleanup the test files
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true); // Clean up the entire test folder
                }
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCreateDirectories_RealFileSystem()
        {
            // Arrange
            var component = new Component("TestZipWithDirectories", ComponentType.Zip, ComponentAction.None);
            var tempFolder = Path.Combine(Path.GetTempPath(), "InstallerPluginCreateDirTests");
            Directory.CreateDirectory(tempFolder); // Ensure the directory exists

            var sourceZipPath = Path.Combine(tempFolder, "testWithDirectories.zip");
            var targetDir = Path.Combine(tempFolder, "output");

            component.Parameters["SourceFile"] = sourceZipPath;
            component.Parameters["TargetDir"] = targetDir;

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestZipWithDirectories" } };

            // Create a zip file dynamically with directories
            using (var zipFileStream = new FileStream(sourceZipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    // Directory entries
                    CreateZipEntry(archive, "folder1/");
                    CreateZipEntry(archive, "folder2/subfolder/");

                    // Files within directories
                    CreateZipEntry(archive, "folder1/file1.txt", "File inside folder1");
                    CreateZipEntry(archive, "folder2/subfolder/file2.txt", "File inside folder2/subfolder");
                }
            }

            try
            {
                // Act
                var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "The plugin should have successfully extracted the ZIP component, including directories.");

                // Verify that directories were created
                Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "folder1")), "folder1 should have been created.");
                Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "folder2")), "folder2 should have been created.");
                Assert.IsTrue(Directory.Exists(Path.Combine(targetDir, "folder2", "subfolder")), "subfolder should have been created.");

                // Verify that files within directories were extracted
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "folder1", "file1.txt")), "file1.txt should have been extracted.");
                Assert.IsTrue(File.Exists(Path.Combine(targetDir, "folder2", "subfolder", "file2.txt")), "file2.txt should have been extracted.");
            }
            finally
            {
                // Cleanup the test files
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true); // Clean up the entire test folder
                }
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogErrorAndReturnFalse_WhenMsiExecutionFails()
        {
            // Arrange
            var component = new Component("TestMsi", ComponentType.Msi, ComponentAction.Install);
            component.Parameters["SourceFile"] = "C:\\test.msi";
            component.Parameters["Params"] = "/qn";

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestMsi" } };

            // Simulate a failure in the MSI execution (non-zero exit code)
            _processExecutorMock.Setup(p => p.ExecuteAsync("msiexec.exe", "/i \"C:\\test.msi\" /qn", It.IsAny<string>()))
                .ReturnsAsync(1);  // Exit code 1 means failure

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the MSI execution fails.");

            // Verify the error log is called with the correct message
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("MSI execution failed with exit code 1."))), Times.Once);

            // Verify the process executor was called exactly once
            _processExecutorMock.Verify(p => p.ExecuteAsync("msiexec.exe", "/i \"C:\\test.msi\" /qn", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogErrorAndReturnFalse_WhenBatOrExeExecutionFails()
        {
            // Arrange
            var component = new Component("TestBatOrExe", ComponentType.Bat, ComponentAction.None);
            component.Parameters["SourceFile"] = "C:\\test.bat";
            component.Parameters["Params"] = "/S";

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestBatOrExe" } };

            // Simulate a failure in the BAT/EXE execution (non-zero exit code)
            _processExecutorMock.Setup(p => p.ExecuteAsync("C:\\test.bat", "/S", It.IsAny<string>()))
                .ReturnsAsync(1);  // Exit code 1 means failure

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when the BAT/EXE execution fails.");

            // Verify the error log is called with the correct message
            _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.Contains("Execution failed with exit code 1."))), Times.Once);

            // Verify the process executor was called exactly once
            _processExecutorMock.Verify(p => p.ExecuteAsync("C:\\test.bat", "/S", It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogErrorAndReturnFalse_WhenMsiSourceFileIsMissing()
        {
            // Arrange
            var component = new Component("TestMsi", ComponentType.Msi, ComponentAction.Install);
            // Simulating missing 'SourceFile' by not setting the parameter
            component.Parameters["Params"] = "/qn";

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestMsi" } };

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when 'SourceFile' is missing for msi component.");
            _loggerMock.Verify(l => l.Error(It.Is<Exception>(ex => ex is InvalidOperationException),
                It.Is<string>(msg => msg.Contains("An error occurred during installation/uninstallation."))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogErrorAndReturnFalse_WhenBatOrExeSourceFileIsMissing()
        {
            // Arrange
            var component = new Component("TestBat", ComponentType.Bat, ComponentAction.None);
            // Simulating missing 'SourceFile' by not setting the parameter
            component.Parameters["Params"] = "/S";  // Other params are optional, so we only leave 'SourceFile' missing

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestBat" } };

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when 'SourceFile' is missing for a .bat/.exe component.");
            _loggerMock.Verify(l => l.Error(It.Is<Exception>(ex => ex is InvalidOperationException),
                It.Is<string>(msg => msg.Contains("An error occurred during installation/uninstallation."))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogErrorAndReturnFalse_WhenZipSourceFileOrTargetDirIsMissing()
        {
            // Arrange
            var component = new Component("TestZip", ComponentType.Zip, ComponentAction.None);
            // Simulating missing 'SourceFile' and 'TargetDir'
            component.Parameters["SourceFile"] = null;  // Missing source file
            component.Parameters["TargetDir"] = null;   // Missing target directory

            // Add component to the config
            _pluginConfig.Components.Add(component);

            // Setup the command line arguments for this test
            var commandLineArgs = new Dictionary<string, string> { { "component", "TestZip" } };

            // Act
            var result = await _plugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result, "The plugin should return false when 'SourceFile' or 'TargetDir' is missing for a zip component.");

            // Verify that the correct exception was logged
            _loggerMock.Verify(l => l.Error(It.Is<Exception>(ex => ex is InvalidOperationException),
                It.Is<string>(msg => msg.Contains("An error occurred during installation/uninstallation."))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCancelExecution_WhenCancellationTokenIsTriggeredDuringFileExtraction()
        {
            // Arrange
            var component = new Component("TestZip", ComponentType.Zip, ComponentAction.None);
            var tempFolder = Path.Combine(Path.GetTempPath(), "InstallerPluginTests");
            Directory.CreateDirectory(tempFolder);

            var sourceZipPath = Path.Combine(tempFolder, "test.zip");
            var targetDir = Path.Combine(tempFolder, "output");

            component.Parameters["SourceFile"] = sourceZipPath;
            component.Parameters["TargetDir"] = targetDir;

            // Add the component to the config
            _pluginConfig.Components.Add(component);

            var commandLineArgs = new Dictionary<string, string> { { "component", "TestZip" } };

            // Create a zip file dynamically
            using (var zipFileStream = new FileStream(sourceZipPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var zipEntry = archive.CreateEntry($"test{i}.txt");
                        using (var entryStream = zipEntry.Open())
                        using (var writer = new StreamWriter(entryStream))
                        {
                            await writer.WriteAsync($"This is a test file {i} inside the zip.").ConfigureAwait(false);
                        }
                    }
                }
            }

            // Initialize the plugin with mocks and setup
            var pluginMock = new Mock<InstallerPlugin>(_loggerMock.Object, _configParserMock.Object, _processExecutorMock.Object)
            {
                CallBase = true // Allows real methods except those overridden
            };

            // Setup the config to be initialized
            _configParserMock.Setup(cp => cp.Execute()).Returns(_pluginConfig); // Config is initialized via the parser
            pluginMock.Object.Init(); // Ensure Config is initialized

            // Introduce a delay in the ExtractToFile method to simulate long extraction process
            pluginMock
                .Protected()
                .Setup("OnProgress", ItExpr.IsAny<PluginProgressEventArgs>())
                .Callback(() =>
                {
                    Task.Delay(1000).Wait(); // Simulate long processing time for file extraction
                });

            using (var sourceToken = new CancellationTokenSource())
            {
                sourceToken.CancelAfter(500);
                var result = await pluginMock.Object.ExecuteAsync(commandLineArgs, sourceToken.Token).ConfigureAwait(false);

                // Assert
                Assert.IsFalse(result, "Execution should be cancelled when the plugin is killed.");
            }

            _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("Plugin execution cancelled."))), Times.Once);
        }

        private static void CreateZipEntry(ZipArchive archive, string entryName, string content = null)
        {
            var zipEntry = archive.CreateEntry(entryName);
            if (content != null)
            {
                using (var entryStream = zipEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write(content);
                }
            }
        }
    }
}