using CommandLine;
using Curupira.AppClient;
using Curupira.AppClient.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Curupira.Tests.AppClient
{
    [TestClass]
    public class OptionsTests
    {
        private Mock<IConsoleService> _consoleServiceMock;
        private Options _options;

        [TestInitialize]
        public void Setup()
        {
            _consoleServiceMock = new Mock<IConsoleService>();
            _options = new Options(_consoleServiceMock.Object);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalseAndPrintError_WhenNeitherPluginNorListPluginsIsProvided()
        {
            // Arrange
            _options.Plugin = null;
            _options.ListPlugins = false;
            var parserResult = CreateNotParsedInstance<Options>(new List<Error>());

            // Act
            var result = _options.IsValid(parserResult);

            // Assert
            Assert.IsFalse(result);
            _consoleServiceMock.Verify(c => c.WriteLine("Error: You must provide either '-p' to specify a plugin or '-a' to list available plugins."), Times.Once);
            _consoleServiceMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalseAndPrintError_WhenBothPluginAndListPluginsAreProvided()
        {
            // Arrange
            _options.Plugin = "TestPlugin";
            _options.ListPlugins = true;
            var parserResult = CreateNotParsedInstance<Options>(new List<Error>());

            // Act
            var result = _options.IsValid(parserResult);

            // Assert
            Assert.IsFalse(result);
            _consoleServiceMock.Verify(c => c.WriteLine("Error: You cannot specify both '-a' (list plugins) and '-p' (plugin) at the same time."), Times.Once);
            _consoleServiceMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenOnlyPluginIsProvided()
        {
            // Arrange
            _options.Plugin = "TestPlugin";
            _options.ListPlugins = false;
            var parserResult = CreateParsedInstance<Options>(_options);

            // Act
            var result = _options.IsValid(parserResult);

            // Assert
            Assert.IsTrue(result);
            _consoleServiceMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenOnlyListPluginsIsProvided()
        {
            // Arrange
            _options.Plugin = null;
            _options.ListPlugins = true;
            var parserResult = CreateParsedInstance<Options>(_options);

            // Act
            var result = _options.IsValid(parserResult);

            // Assert
            Assert.IsTrue(result);
            _consoleServiceMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void ToString_ShouldReturnSerializedOptions()
        {
            // Arrange
            _options.Plugin = "TestPlugin";
            _options.Level = "Debug";
            _options.NoLogo = true;
            _options.NoProgressBar = true;
            _options.ListPlugins = false;
            _options.Params = "param1=value1 param2=value2";

            // Act
            var result = _options.ToString();

            // Assert
            Assert.IsTrue(result.Contains("\"Plugin\":\"TestPlugin\""));
            Assert.IsTrue(result.Contains("\"Level\":\"Debug\""));
            Assert.IsTrue(result.Contains("\"NoLogo\":true"));
            Assert.IsTrue(result.Contains("\"NoProgressBar\":true"));
            Assert.IsTrue(result.Contains("\"Params\":\"param1=value1 param2=value2\""));
        }

        private static Parsed<T> CreateParsedInstance<T>(T value)
        {
            // Get the constructor for NotParsed<T> that takes a IEnumerable<Error> as an argument
            var constructor = typeof(Parsed<T>).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(T) },
                null) ?? throw new InvalidOperationException("Could not find the required constructor for Parsed<T>.");

            // Use reflection to invoke the constructor
            var parsedInstance = (Parsed<T>)constructor.Invoke(new object[] { value });

            return parsedInstance;
        }

        private static NotParsed<T> CreateNotParsedInstance<T>(IEnumerable<Error> errors)
        {
            // Get the constructor for NotParsed<T> that takes a IEnumerable<Error> as an argument
            var constructor = typeof(NotParsed<T>).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(CommandLine.TypeInfo), typeof(IEnumerable<Error>) },
                null) ?? throw new InvalidOperationException("Could not find the required constructor for NotParsed<T>.");

            // Use reflection to invoke the constructor
            var notParsedInstance = (NotParsed<T>)constructor.Invoke(new object[] { null, errors });

            return notParsedInstance;
        }
    }
}
