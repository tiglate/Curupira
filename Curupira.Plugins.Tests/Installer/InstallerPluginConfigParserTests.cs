using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Curupira.Plugins.Installer;

namespace Curupira.Plugins.Tests.Installer
{
    [TestClass]
    public class InstallerPluginConfigParserTests
    {
        private string _tempFilePath;

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        private void CreateTempConfigFile(string xmlContent)
        {
            _tempFilePath = Path.GetTempFileName();
            File.WriteAllText(_tempFilePath, xmlContent);
        }

        [TestMethod]
        public void Execute_ShouldParseComponentsCorrectly()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='ruby' type='zip'>
                        <param name='SourceFile' value='C:\temp\ruby.zip' />
                        <param name='TargetDir' value='P:\Ruby' />
                        <remove>include\ruby-3.2.0\ruby.h</remove>
                        <remove>include\ruby-3.2.0\ruby\io\*</remove>
                    </component>
                    <component id='InstallAppTest' type='msi' action='install'>
                        <param name='SourceFile' value='C:\temp\AppTeste.msi' />
                        <param name='Params' value='/qn' />
                    </component>
                    <component id='Install.bat' type='bat'>
                        <param name='SourceFile' value='C:\temp\Install.bat' />
                        <param name='Params' value='C:\temp\myapp' />
                    </component>
                    <component id='Installer.exe' type='exe'>
                        <param name='SourceFile' value='C:\temp\Installer.exe' />
                        <param name='Params' value='C:\temp\myapp2' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            var result = configParser.Execute();

            // Assert
            Assert.AreEqual(4, result.Components.Count, "Expected 4 components to be parsed.");

            var rubyComponent = result.Components[0];
            Assert.AreEqual("ruby", rubyComponent.Id);
            Assert.AreEqual(ComponentType.Zip, rubyComponent.Type);
            Assert.AreEqual(2, rubyComponent.RemoveItems.Count);
            Assert.AreEqual(@"include\ruby-3.2.0\ruby.h", rubyComponent.RemoveItems[0]);
            Assert.AreEqual(@"include\ruby-3.2.0\ruby\io\*", rubyComponent.RemoveItems[1]);

            var msiComponent = result.Components[1];
            Assert.AreEqual("InstallAppTest", msiComponent.Id);
            Assert.AreEqual(ComponentType.Msi, msiComponent.Type);
            Assert.AreEqual(ComponentAction.Install, msiComponent.Action);

            var batComponent = result.Components[2];
            Assert.AreEqual("Install.bat", batComponent.Id);
            Assert.AreEqual(ComponentType.Bat, batComponent.Type);
            Assert.AreEqual(ComponentAction.None, batComponent.Action);

            var exeComponent = result.Components[3];
            Assert.AreEqual("Installer.exe", exeComponent.Id);
            Assert.AreEqual(ComponentType.Exe, exeComponent.Type);
            Assert.AreEqual(ComponentAction.None, exeComponent.Action);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenComponentIdOrTypeIsMissing()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='' type='zip'>
                        <param name='SourceFile' value='C:\temp\ruby.zip' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            Assert.ThrowsException<InvalidOperationException>(configParser.Execute);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenInvalidComponentTypeIsProvided()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='TestComponent' type='unknown'>
                        <param name='SourceFile' value='C:\temp\test.zip' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            Assert.ThrowsException<InvalidOperationException>(configParser.Execute);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenMsiComponentActionIsMissing()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='TestMsi' type='msi'>
                        <param name='SourceFile' value='C:\temp\AppTeste.msi' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            Assert.ThrowsException<InvalidOperationException>(configParser.Execute);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenMsiComponentHasInvalidAction()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='TestMsi' type='msi' action='invalidAction'>
                        <param name='SourceFile' value='C:\temp\AppTeste.msi' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            Assert.ThrowsException<InvalidOperationException>(configParser.Execute);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenParamNameIsMissing()
        {
            // Arrange
            var xmlContent = @"
            <plugin xmlns='http://ampliar.dev.br/projects/curupira/plugin/installer-plugin'>
                <components>
                    <component id='TestComponent' type='zip'>
                        <param value='C:\temp\ruby.zip' />
                    </component>
                </components>
            </plugin>";

            CreateTempConfigFile(xmlContent);
            var configParser = new InstallerPluginConfigParser(_tempFilePath);

            // Act
            Assert.ThrowsException<InvalidOperationException>(configParser.Execute);
        }
    }
}
