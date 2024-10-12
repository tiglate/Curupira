using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Curupira.Plugins.ServiceManager;

namespace Curupira.Plugins.Tests.ServiceManager
{
    [TestClass]
    public class ServiceManagerPluginConfigParserTests
    {
        private const string ValidXmlContent = @"<?xml version='1.0' encoding='utf-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin service-manager-plugin.xsd'>
                <bundles>
                    <bundle id='stop_all'>
                        <service name='WSearch' action='Stop' />
                        <service name='wuauserv' action='Stop' />
                    </bundle>
                    <bundle id='start_all'>
                        <service name='WSearch' action='Start' />
                        <service name='wuauserv' action='Start' />
                    </bundle>
                    <bundle id='status_test' logFile='c:\temp\{0:yyyy-MM-dd}-services.txt'>
                        <service name='VSSERV' action='Status' />
                        <service name='W32Time' action='Status' />
                        <service name='WaaSMedicSvc' action='Status' />
                    </bundle>
                </bundles>
            </plugin>";

        private const string MissingIdAttributeXml = @"<?xml version='1.0' encoding='utf-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin service-manager-plugin.xsd'>
                <bundles>
                    <bundle>
                        <service name='WSearch' action='Stop' />
                    </bundle>
                </bundles>
            </plugin>";

        private const string InvalidActionAttributeXml = @"<?xml version='1.0' encoding='utf-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin service-manager-plugin.xsd'>
                <bundles>
                    <bundle id='stop_all'>
                        <service name='WSearch' action='UnknownAction' />
                    </bundle>
                </bundles>
            </plugin>";

        private static string CreateTemporaryFileWithContent(string content)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        [TestMethod]
        public void Execute_ShouldParseValidXmlCorrectly()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(ValidXmlContent);
            var parser = new ServiceManagerPluginConfigParser(tempFile);

            // Act
            var config = parser.Execute();

            // Assert
            Assert.AreEqual(3, config.Bundles.Count);

            // Check stop_all bundle
            Assert.IsTrue(config.Bundles.ContainsKey("stop_all"));
            var stopAllBundle = config.Bundles["stop_all"];
            Assert.AreEqual(2, stopAllBundle.Services.Count);
            Assert.AreEqual("WSearch", stopAllBundle.Services[0].ServiceName);
            Assert.AreEqual(Curupira.Plugins.ServiceManager.Action.Stop, stopAllBundle.Services[0].Action);
            Assert.AreEqual("wuauserv", stopAllBundle.Services[1].ServiceName);
            Assert.AreEqual(Curupira.Plugins.ServiceManager.Action.Stop, stopAllBundle.Services[1].Action);

            // Check status_test bundle with logFile
            Assert.IsTrue(config.Bundles.ContainsKey("status_test"));
            var statusTestBundle = config.Bundles["status_test"];
            Assert.AreEqual("c:\\temp\\{0:yyyy-MM-dd}-services.txt", statusTestBundle.LogFile);
            Assert.AreEqual(3, statusTestBundle.Services.Count);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenBundleIdIsMissing()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(MissingIdAttributeXml);
            var parser = new ServiceManagerPluginConfigParser(tempFile);

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => parser.Execute());
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenActionIsInvalid()
        {
            // Arrange
            var tempFile = CreateTemporaryFileWithContent(InvalidActionAttributeXml);
            var parser = new ServiceManagerPluginConfigParser(tempFile);

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => parser.Execute());
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenServiceNameIsMissing()
        {
            // Arrange
            const string missingServiceNameXml = @"<?xml version='1.0' encoding='utf-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin service-manager-plugin.xsd'>
                <bundles>
                    <bundle id='stop_all'>
                        <service action='Stop' />
                    </bundle>
                </bundles>
            </plugin>";

            var tempFile = CreateTemporaryFileWithContent(missingServiceNameXml);
            var parser = new ServiceManagerPluginConfigParser(tempFile);

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => parser.Execute()); // Should throw InvalidOperationException because the service name is missing
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenServiceActionIsMissing()
        {
            // Arrange
            const string missingServiceActionXml = @"<?xml version='1.0' encoding='utf-8' ?>
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xsi:schemaLocation='http://ampliar.dev.br/projects/curupira/plugin/service-manager-plugin service-manager-plugin.xsd'>
                <bundles>
                    <bundle id='stop_all'>
                        <service name='WSearch' />
                    </bundle>
                </bundles>
            </plugin>";

            var tempFile = CreateTemporaryFileWithContent(missingServiceActionXml);
            var parser = new ServiceManagerPluginConfigParser(tempFile);

            // Act
            Assert.ThrowsException<InvalidOperationException>(() => parser.Execute()); // Should throw InvalidOperationException because the service action is missing
        }


        [TestCleanup]
        public void Cleanup()
        {
            // Clean up any temporary files created during the tests
            foreach (var tempFile in Directory.GetFiles(Path.GetTempPath(), "*.tmp"))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch { /* Ignore exceptions during cleanup */ }
            }
        }
    }
}
