using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Curupira.Plugins.FoldersCreator;

namespace Curupira.Plugins.Tests.FoldersCreator
{
    [TestClass]
    public class FoldersCreatorPluginConfigParserTests
    {
        private const string ValidXmlContent = @"<?xml version='1.0' encoding='UTF-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin folders-creator-plugin.xsd'>
                <directories>
                    <add>C:\temp\myapp\bin</add>
                    <add>C:\temp\myapp\backup\logs</add>
                    <add>C:\temp\myapp\other</add>
                    <add>\\192.168.1.1\abcd\folder\newfolder</add>
                </directories>
            </plugin>";

        private const string LinuxStyleXmlContent = @"<?xml version='1.0' encoding='UTF-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin folders-creator-plugin.xsd'>
                <directories>
                    <add>/home/user/folder</add>
                </directories>
            </plugin>";

        private const string XmlWithMixedSeparatorsContent = @"<?xml version='1.0' encoding='UTF-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/folders-creator-plugin folders-creator-plugin.xsd'>
                <directories>
                    <add>C:/temp/myapp/bin</add>
                    <add>C:/temp/myapp/backup/logs</add>
                </directories>
            </plugin>";

        private static string CreateTemporaryFileWithContent(string content)
        {
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, content);
            return tempFile;
        }

        [TestMethod]
        public void Execute_ShouldParseValidXmlCorrectly()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(ValidXmlContent);
            var parser = new FoldersCreatorPluginConfigParser(tempFile);

            // Act
            var config = parser.Execute();

            // Assert
            Assert.AreEqual(4, config.DirectoriesToCreate.Count);
            CollectionAssert.AreEquivalent(new List<string>
            {
                @"C:\temp\myapp\bin",
                @"C:\temp\myapp\backup\logs",
                @"C:\temp\myapp\other",
                @"\\192.168.1.1\abcd\folder\newfolder"
            }, (System.Collections.ICollection)config.DirectoriesToCreate);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenLinuxDirectoryIsPresent()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(LinuxStyleXmlContent);
            var parser = new FoldersCreatorPluginConfigParser(tempFile);

            // Act
            Assert.ThrowsException<NotSupportedException>(() => parser.Execute());
        }

        [TestMethod]
        public void Execute_ShouldReplaceForwardSlashesWithBackslashes()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(XmlWithMixedSeparatorsContent);
            var parser = new FoldersCreatorPluginConfigParser(tempFile);

            // Act
            var config = parser.Execute();

            // Assert
            Assert.AreEqual(2, config.DirectoriesToCreate.Count);
            CollectionAssert.AreEquivalent(new List<string>
            {
                @"C:\temp\myapp\bin",
                @"C:\temp\myapp\backup\logs"
            }, (System.Collections.ICollection)config.DirectoriesToCreate);
        }

        [TestMethod]
        public void Execute_ShouldThrowArgumentNullException_WhenXmlConfigIsNull()
        {
            // Arrange
            var parser = new FoldersCreatorPluginConfigParser(null);

            // Act
            Assert.ThrowsException<ArgumentNullException>(() =>  parser.Execute());
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up any temporary files created during the tests
            foreach (var tempFile in System.IO.Directory.GetFiles(System.IO.Path.GetTempPath(), "*.tmp"))
            {
                try
                {
                    System.IO.File.Delete(tempFile);
                }
                catch { /* Ignore exceptions on cleanup */ }
            }
        }
    }
}
