using System;
using Curupira.Plugins.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Curupira.Tests.Plugins.Common
{
    [TestClass]
    public class FileSystemHelperTests
    {
        [TestMethod]
        [DataRow(@"C:\nonexistent\folder", @"C:")]
        [DataRow(@"C:\nonexistent", @"C:")]
        [DataRow(@"C:\Windows\System32", @"C:\Windows\System32")]
        [DataRow(@"Z:\", @"")]
        public void GetFirstExistingDirectoryOrRoot_ShouldReturnFirstExistingDirectoryOrRoot(string input, string expected)
        {
            // Arrange
            // We can't mock Directory.Exists easily, so we'll skip the actual existence test and focus on logical behavior

            // Act
            var result = FileSystemHelper.GetFirstExistingDirectoryOrRoot(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DirectoryExists_ShouldReturnTrue_WhenDirectoryExists()
        {
            // Arrange
            var path = AppDomain.CurrentDomain.BaseDirectory; // Use the current directory for the test

            // Act
            var result = FileSystemHelper.DirectoryExists(path);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DirectoryExists_ShouldReturnFalse_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var path = @"C:\nonexistent\directory";

            // Act
            var result = FileSystemHelper.DirectoryExists(path);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow(@"C:\folder\subfolder", @"C:\folder")]
        [DataRow(@"C:\folder", @"C:")]
        [DataRow(@"C:\", @"C:")]
        [DataRow(@"\\network\share\folder", @"\\network\share")]
        [DataRow(@"\\network\share\", @"\\network")]
        [DataRow(@"\\192.168.1.1", @"\\192.168.1.1")]
        public void GetParentDirectory_ShouldReturnParentDirectory(string input, string expected)
        {
            // Act
            var result = FileSystemHelper.GetParentDirectory(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetParentDirectory_ShouldThrowArgumentNullException_WhenPathIsNull()
        {
            // Act
            FileSystemHelper.GetParentDirectory(null);

            // Assert is handled by the ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetParentDirectory_ShouldThrowArgumentNullException_WhenPathIsEmpty()
        {
            // Act
            FileSystemHelper.GetParentDirectory(string.Empty);

            // Assert is handled by the ExpectedException attribute
        }
    }
}
