using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Curupira.Plugins.Backup;
using System;

namespace Curupira.Tests.Plugins.Backup
{
    [TestClass]
    public class FileMatcherTests
    {
        private static string _testRootDir = @"C:\Temp\FileMatcherTests";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Directory.CreateDirectory(_testRootDir);
            Directory.CreateDirectory(Path.Combine(_testRootDir, "subfolder"));
            File.WriteAllText(Path.Combine(_testRootDir, "file.txt"), "Some text content");
            File.WriteAllText(Path.Combine(_testRootDir, "subfolder", "anotherfile.txt"), "More text content");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (Directory.Exists(_testRootDir))
            {
                Directory.Delete(_testRootDir, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullRootDir_ThrowsArgumentNullException()
        {
            new FileMatcher(null, new List<string> { "pattern" }); // Should throw ArgumentNullException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyRootDir_ThrowsArgumentNullException()
        {
            new FileMatcher("", new List<string> { "pattern" }); // Should throw ArgumentNullException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhitespaceRootDir_ThrowsArgumentNullException()
        {
            new FileMatcher("   ", new List<string> { "pattern" }); // Should throw ArgumentNullException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullPatterns_ThrowsArgumentNullException()
        {
            new FileMatcher(@"C:\MyFolder", null); // Should throw ArgumentNullException
        }

        [TestMethod]
        public void IsMatch_EmptyPath_ReturnsFalse()
        {
            var patterns = new List<string> { "file.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            Assert.IsFalse(matcher.IsMatch(null));
            Assert.IsFalse(matcher.IsMatch(string.Empty));
            Assert.IsFalse(matcher.IsMatch(" "));
        }

        [TestMethod]
        public void IsMatch_ExactMatch_ReturnsTrue()
        {
            var patterns = new List<string> { "file.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            bool result = matcher.IsMatch(Path.Combine(_testRootDir, "file.txt"));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMatch_WildcardMatch_ReturnsTrue()
        {
            var patterns = new List<string> { "*.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            bool result = matcher.IsMatch(Path.Combine(_testRootDir, "subfolder", "anotherfile.txt"));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMatch_NoMatch_ReturnsFalse()
        {
            var patterns = new List<string> { "*.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            bool result = matcher.IsMatch(Path.Combine(_testRootDir, "subfolder", "image.jpg"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsMatch_OutsideRoot_ReturnsFalse()
        {
            var patterns = new List<string> { "*.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            // Create a directory outside the test root
            string outsideDir = Path.Combine(Path.GetDirectoryName(_testRootDir), "OtherFolder");
            Directory.CreateDirectory(outsideDir);
            string outsideFilePath = Path.Combine(outsideDir, "file.txt");
            File.WriteAllText(outsideFilePath, "Some text content");

            bool result = matcher.IsMatch(outsideFilePath);

            Assert.IsFalse(result);

            // Clean up the outside directory
            File.Delete(outsideFilePath);
            Directory.Delete(outsideDir);
        }

        [TestMethod]
        public void IsMatch_MultiplePatterns_ReturnsTrueIfAnyMatch()
        {
            var patterns = new List<string> { "*.txt", "specific_folder/*" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            // Create the "specific_folder" and a file within it
            string specificFolderPath = Path.Combine(_testRootDir, "specific_folder");
            Directory.CreateDirectory(specificFolderPath);
            File.WriteAllText(Path.Combine(specificFolderPath, "anyfile.exe"), "Some content");

            bool result1 = matcher.IsMatch(Path.Combine(_testRootDir, "subfolder", "anotherfile.txt"));
            bool result2 = matcher.IsMatch(Path.Combine(specificFolderPath, "anyfile.exe"));

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);

            // Clean up the specific folder
            File.Delete(Path.Combine(specificFolderPath, "anyfile.exe"));
            Directory.Delete(specificFolderPath);
        }

        [TestMethod]
        public void IsMatch_CaseInsensitive_ReturnsTrue()
        {
            var patterns = new List<string> { "FILE.TXT" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            bool result = matcher.IsMatch(Path.Combine(_testRootDir, "file.txt"));

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMatch_WildcardInMiddle_ReturnsTrue()
        {
            var patterns = new List<string> { "folderA/*/file.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            // Create the necessary directory structure and file
            string folderAPath = Path.Combine(_testRootDir, "folderA");
            string subfolderPath = Path.Combine(folderAPath, "subfolder");
            Directory.CreateDirectory(subfolderPath);
            File.WriteAllText(Path.Combine(subfolderPath, "file.txt"), "Some content");

            bool result = matcher.IsMatch(Path.Combine(subfolderPath, "file.txt"));

            Assert.IsTrue(result);

            // Clean up
            File.Delete(Path.Combine(subfolderPath, "file.txt"));
            Directory.Delete(subfolderPath);
            Directory.Delete(folderAPath);
        }

        [TestMethod]
        public void IsMatch_MultipleWildcards_ReturnsTrue()
        {
            var patterns = new List<string> { "**/*.txt" };
            var matcher = new FileMatcher(_testRootDir, patterns);

            // Create the necessary directory structure and file
            string folderAPath = Path.Combine(_testRootDir, "folderA");
            string subfolderPath = Path.Combine(folderAPath, "subfolder");
            string deeperPath = Path.Combine(subfolderPath, "deeper");
            Directory.CreateDirectory(deeperPath);
            File.WriteAllText(Path.Combine(deeperPath, "file.txt"), "Some content");

            bool result = matcher.IsMatch(Path.Combine(deeperPath, "file.txt"));

            Assert.IsTrue(result);

            // Clean up
            File.Delete(Path.Combine(deeperPath, "file.txt"));
            Directory.Delete(deeperPath);
            Directory.Delete(subfolderPath);
            Directory.Delete(folderAPath);
        }
    }
}