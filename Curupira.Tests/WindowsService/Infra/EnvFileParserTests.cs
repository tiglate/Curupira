using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Curupira.WindowsService.Infra;

namespace Curupira.WindowsService.Tests.Infra
{
    [TestClass]
    public class EnvFileParserTests
    {
        private string _tempFilePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.env");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_ShouldThrowArgumentException_WhenFilePathIsNullOrEmpty()
        {
            EnvFileParser.Parse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Parse_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            EnvFileParser.Parse(_tempFilePath);
        }

        [TestMethod]
        public void Parse_ShouldReturnEmptyDictionary_WhenFileContainsOnlyComments()
        {
            File.WriteAllText(_tempFilePath, "# This is a comment\n# Another comment");

            var result = EnvFileParser.Parse(_tempFilePath);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Parse_ShouldParseVariablesCorrectly()
        {
            var content = @"
                # This is a comment
                key1=value1
                key2 = value2
                key3= value3
                key4 =value4
                key5 =  value5
                key6=value6 # inline comment
                key7 = value7 # another inline comment
                ";
            File.WriteAllText(_tempFilePath, content);

            var result = EnvFileParser.Parse(_tempFilePath);

            var expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" },
                { "key4", "value4" },
                { "key5", "value5" },
                { "key6", "value6" },
                { "key7", "value7" }
            };

            CollectionAssert.AreEquivalent(expected, result);
        }

        [TestMethod]
        public void Parse_ShouldIgnoreEmptyLines()
        {
            var content = @"
                key1=value1

                key2=value2
                ";
            File.WriteAllText(_tempFilePath, content);

            var result = EnvFileParser.Parse(_tempFilePath);

            var expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}
