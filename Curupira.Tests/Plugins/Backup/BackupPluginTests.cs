using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Curupira.Plugins.Backup;
using Curupira.Plugins.Contract;
using System.IO.Compression;
using System.Threading;

namespace Curupira.Tests.Plugins.Backup
{
    [TestClass]
    public class BackupPluginTests
    {
        private string _tempFolder;
        private string _destinationFolder;
        private string _filesFolder;
        private string _filesSubFolder;
        private BackupPluginConfig _backupPluginConfig;

        [TestInitialize]
        public void Setup()
        {
            // Set up temporary folder for testing
            _tempFolder = Path.Combine(Path.GetTempPath(), "BackupPluginTest");
            _filesFolder = Path.Combine(_tempFolder, "Files");
            _filesSubFolder = Path.Combine(_filesFolder, "subfolder");
            _destinationFolder = Path.Combine(_tempFolder, "Backups");

            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }

            Directory.CreateDirectory(_tempFolder);
            Directory.CreateDirectory(_filesFolder);
            Directory.CreateDirectory(_filesSubFolder);
            Directory.CreateDirectory(_destinationFolder);

            // Create a dummy config with files to back up
            _backupPluginConfig = new BackupPluginConfig
            {
                Destination = _destinationFolder,
                Limit = 3
            };

            // Add archives to the read-only list
            _backupPluginConfig.Archives.Add(new BackupArchive("testArchive", _filesFolder));

            // Create dummy files to back up
            CreateDummyFiles(_filesFolder, 5);
            CreateDummyFiles(_filesSubFolder, 5);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up files and folders
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldCreateBackupSuccessfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            configParserMock.Setup(p => p.Execute()).Returns(_backupPluginConfig);

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                var commandLineArgs = new Dictionary<string, string>();

                // Act
                var result = await backupPlugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "Backup should be created successfully.");
            }
            Assert.AreEqual(1, Directory.GetFiles(_destinationFolder, "*.zip").Length, "A zip file should be created.");
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldDeleteOldestBackupWhenLimitExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            configParserMock.Setup(p => p.Execute()).Returns(_backupPluginConfig);

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                // Create initial backups to exceed limit
                for (int i = 0; i < backupPlugin.Config.Limit; i++)
                {
                    var zipFileName = $"{DateTime.Now.AddDays(-i):yyyyMMddhhmmss}-testArchive.zip";
                    using (var stream = File.Open(Path.Combine(_destinationFolder, zipFileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                }

                // Act
                var result = await backupPlugin.ExecuteAsync(new Dictionary<string, string>()).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "Backup should be created successfully.");
            }
            Assert.AreEqual(3, Directory.GetFiles(_destinationFolder, "*.zip").Length, "Only the latest 3 backups should remain.");
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldHandleNoFilesToBackup()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            configParserMock.Setup(p => p.Execute()).Returns(new BackupPluginConfig
            {
                Destination = _destinationFolder,
                Limit = 3
            });

            // Add an archive with an empty folder
            var emptyArchive = new BackupArchive("emptyArchive", Path.Combine(_tempFolder, "Empty"));
            Directory.CreateDirectory(Path.Combine(_tempFolder, "Empty"));

            var config = configParserMock.Object.Execute();
            config.Archives.Add(emptyArchive);

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                // Act
                var result = await backupPlugin.ExecuteAsync(new Dictionary<string, string>()).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "Backup should succeed even if there are no files to back up.");
            }
            Assert.AreEqual(1, Directory.GetFiles(_destinationFolder, "*.zip").Length, "A zip file should be created for the empty archive.");
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldCancelMidExecution_WithPartialMock()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();

            // Setup Backup Config with a large number of files
            _backupPluginConfig = new BackupPluginConfig
            {
                Destination = _destinationFolder,
                Limit = 3
            };

            // Add a valid archive with a large number of dummy files
            _backupPluginConfig.Archives.Add(new BackupArchive("testArchive", _tempFolder));
            CreateDummyFiles(_tempFolder, 10); // Simulate 10 files to process

            configParserMock.Setup(p => p.Execute()).Returns(_backupPluginConfig);

            // Create a partial mock of BackupPlugin
            var backupPluginMock = new Mock<BackupPlugin>(loggerMock.Object, configParserMock.Object) { CallBase = true };

            // Mock the AddItemsToZip method to simulate a delay
            backupPluginMock
                .Setup(p => p.AddItemToZip(It.IsAny<ZipArchive>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    // Simulate a long-running task by adding a delay inside the overridden method
                    Task.Delay(2000).Wait(); // 2-second delay
                });

            backupPluginMock.Object.Init();

            var commandLineArgs = new Dictionary<string, string>
            {
                { "backup", "testArchive" }
            };

            // Act
            using (var sourceToken = new CancellationTokenSource())
            {
                sourceToken.CancelAfter(500);

                var result = await backupPluginMock.Object.ExecuteAsync(commandLineArgs, sourceToken.Token).ConfigureAwait(false);

                // Assert
                Assert.IsFalse(result, "Backup should stop when the cancellation token is trigged.");
            }

            loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains("Plugin execution cancelled."))));
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldReturnFalse_WhenDestinationIsInvalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            configParserMock.Setup(p => p.Execute()).Returns(new BackupPluginConfig
            {
                Destination = "Z:\\InvalidPath", // Invalid path to trigger error
                Limit = 3
            });

            var config = configParserMock.Object.Execute();
            config.Archives.Add(new BackupArchive("testArchive", _filesFolder));

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                // Act
                var result = await backupPlugin.ExecuteAsync(new Dictionary<string, string>()).ConfigureAwait(false);

                // Assert
                Assert.IsFalse(result, "Backup should fail if the destination path is invalid.");
            }
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(msg => msg.Contains("error occurred during backup"))), Times.Once);
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldProcessSpecificBackup_WhenValidBackupIdProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            var backupId = "testArchive"; // valid id for the test
            _backupPluginConfig = new BackupPluginConfig
            {
                Destination = _destinationFolder,
                Limit = 3
            };
            _backupPluginConfig.Archives.Add(new BackupArchive(backupId, _filesFolder));

            configParserMock.Setup(p => p.Execute()).Returns(_backupPluginConfig);

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                var commandLineArgs = new Dictionary<string, string>
                {
                    { "backup", backupId } // or "archive"
                };

                // Act
                var result = await backupPlugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsTrue(result, "Backup should be processed successfully for the provided backup id.");
            }
            Assert.AreEqual(1, Directory.GetFiles(_destinationFolder, "*.zip").Length, "A zip file should be created for the specific backup.");
            loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.Contains($"Backup '{backupId}' created successfully"))), Times.Once);
        }

        [TestMethod]
        public async Task BackupPlugin_ExecuteAsync_ShouldReturnFalse_WhenInvalidBackupIdProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogProvider>();
            var configParserMock = new Mock<IPluginConfigParser<BackupPluginConfig>>();
            var validBackupId = "testArchive";
            var invalidBackupId = "invalidArchive"; // id does not exist in the config

            _backupPluginConfig = new BackupPluginConfig
            {
                Destination = _destinationFolder,
                Limit = 3
            };
            _backupPluginConfig.Archives.Add(new BackupArchive(validBackupId, _filesFolder));

            configParserMock.Setup(p => p.Execute()).Returns(_backupPluginConfig);

            using (var backupPlugin = new BackupPlugin(loggerMock.Object, configParserMock.Object))
            {
                backupPlugin.Init();

                var commandLineArgs = new Dictionary<string, string>
                {
                    { "backup", invalidBackupId } // or "archive"
                };

                // Act
                var result = await backupPlugin.ExecuteAsync(commandLineArgs).ConfigureAwait(false);

                // Assert
                Assert.IsFalse(result, "Backup should not be processed for an invalid backup id.");
            }
            Assert.AreEqual(0, Directory.GetFiles(_destinationFolder, "*.zip").Length, "No zip file should be created.");
            loggerMock.Verify(l => l.Fatal(It.Is<string>(msg => msg.Contains($"Archive '{invalidBackupId}' not found"))), Times.Once);
        }

        private static void CreateDummyFiles(string rootFolder, int fileCount)
        {
            for (int i = 1; i <= fileCount; i++)
            {
                var filePath = Path.Combine(rootFolder, $"dummyFile{i}.txt");
                File.WriteAllText(filePath, $"This is dummy file {i}");
            }
        }
    }
}
