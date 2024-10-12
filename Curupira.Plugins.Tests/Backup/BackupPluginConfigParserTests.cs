using System;
using System.IO;
using Curupira.Plugins.Backup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Curupira.Plugins.Tests.Backup
{
    [TestClass]
    public class BackupPluginConfigParserTests
    {
        private string _tempConfigFilePath;
        private string _commonFolder;

        [TestInitialize]
        public void Setup()
        {
            _tempConfigFilePath = Path.GetTempFileName();
            _commonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YourAppFolder");

            if (!Directory.Exists(_commonFolder))
            {
                Directory.CreateDirectory(_commonFolder);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempConfigFilePath))
            {
                File.Delete(_tempConfigFilePath);
            }

            if (Directory.Exists(_commonFolder))
            {
                Directory.Delete(_commonFolder, true);
            }
        }

        [TestMethod]
        public void Execute_ShouldReturnValidBackupPluginConfig_WhenValidXmlProvided()
        {
            var validXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='3' />
                    <backups>
                        <backup id='ruby' root='{_commonFolder}\Ruby'>
                            <remove>bin</remove>
                        </backup>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, validXml);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            var config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(_commonFolder, config.Destination);
            Assert.AreEqual(3, config.Limit);
            Assert.AreEqual(1, config.Archives.Count);
            Assert.AreEqual("ruby", config.Archives[0].Id);
            Assert.AreEqual($"{_commonFolder}\\Ruby", config.Archives[0].Root);
            Assert.AreEqual("bin", config.Archives[0].Exclusions[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void Execute_ShouldThrowDirectoryNotFoundException_WhenInvalidDestinationProvided()
        {
            var invalidDestinationXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='invalid\path' limit='3' />
                    <backups>
                        <backup id='ruby' root='{_commonFolder}\Ruby'>
                            <remove>bin</remove>
                        </backup>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, invalidDestinationXml);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            parser.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_ShouldThrowInvalidOperationException_WhenMissingRootInBackupNode()
        {
            var missingRootXml = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='3' />
                    <backups>
                        <backup id='ruby'>
                            <remove>bin</remove>
                        </backup>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, missingRootXml);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            parser.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Execute_ShouldThrowArgumentNullException_WhenNullXmlElementIsPassed()
        {
            var parser = new BackupPluginConfigParser(_tempConfigFilePath);

            // Call private method using reflection for testing.
            var method = typeof(BackupPluginConfigParser).GetMethod("Execute", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            try
            {
                method.Invoke(parser, new object[] { null });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw ex;
            }
        }

        [TestMethod]
        public void Execute_ShouldReturnDefaultConfig_WhenNoBackupsSectionExists()
        {
            var xmlWithoutBackups = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='3' />
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithoutBackups);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            var config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.Archives.Count);
        }

        [TestMethod]
        public void Execute_ShouldSetLimitToZero_WhenLimitIsEmptyOrMissing()
        {
            // Test case where "limit" attribute is empty
            var xmlWithEmptyLimit = $@"<?xml version='1.0' encoding='UTF-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                <settings destination='{_commonFolder}' limit='' />
                <backups>
                    <backup id='ruby' root='{_commonFolder}\Ruby'>
                        <remove>bin</remove>
                    </backup>
                </backups>
            </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithEmptyLimit);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            var config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.Limit, "Limit should default to 0 when the attribute is empty.");

            // Test case where "limit" attribute is missing
            var xmlWithNoLimit = $@"<?xml version='1.0' encoding='UTF-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                <settings destination='{_commonFolder}' />
                <backups>
                    <backup id='ruby' root='{_commonFolder}\Ruby'>
                        <remove>bin</remove>
                    </backup>
                </backups>
            </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithNoLimit);

            parser = new BackupPluginConfigParser(_tempConfigFilePath);
            config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.Limit, "Limit should default to 0 when the attribute is missing.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_ShouldThrowInvalidOperationException_WhenLimitIsNegative()
        {
            var xmlWithNegativeLimit = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='-1' />
                    <backups>
                        <backup id='ruby' root='{_commonFolder}\Ruby'>
                            <remove>bin</remove>
                        </backup>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithNegativeLimit);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            parser.Execute();
        }

        [TestMethod]
        public void Execute_ShouldReturnConfig_WhenBackupsElementIsMissing()
        {
            var xmlWithoutBackupsElement = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='3' />
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithoutBackupsElement);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            var config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.Archives.Count); // No <backup> elements, so Archives should be empty.
        }

        [TestMethod]
        public void Execute_ShouldReturnConfig_WhenNoBackupElementsExist()
        {
            var xmlWithEmptyBackupsElement = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings destination='{_commonFolder}' limit='3' />
                    <backups>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithEmptyBackupsElement);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            var config = parser.Execute();

            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.Archives.Count); // No <backup> elements within <backups>, so Archives should be empty.
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_ShouldThrowInvalidOperationException_WhenDestinationMissingInSettingsAndBackup()
        {
            var xmlWithNoDestination = $@"<?xml version='1.0' encoding='UTF-8' ?>
                <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/backup-plugin backup-plugin.xsd'>
                    <settings limit='3' />
                    <backups>
                        <backup id='ruby' root='{_commonFolder}\Ruby'>
                            <remove>bin</remove>
                        </backup>
                    </backups>
                </plugin>";

            File.WriteAllText(_tempConfigFilePath, xmlWithNoDestination);

            var parser = new BackupPluginConfigParser(_tempConfigFilePath);
            parser.Execute();
        }
    }
}
