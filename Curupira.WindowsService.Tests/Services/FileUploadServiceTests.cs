using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;
using Curupira.WindowsService.Services;

namespace Curupira.WindowsService.Tests.Services
{
    [TestClass]
    public class FileUploadServiceTests
    {
        private string _testDirectory;
        private FileUploadService _fileUploadService;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            ConfigurationManager.AppSettings["UploadDirectory"] = _testDirectory;
            _fileUploadService = new FileUploadService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenUploadDirectoryNotConfigured()
        {
            ConfigurationManager.AppSettings["UploadDirectory"] = null;
            Assert.ThrowsException<ConfigurationErrorsException>(() => new FileUploadService());
        }

        [TestMethod]
        public void Constructor_ShouldCreateDirectory_WhenDirectoryDoesNotExist()
        {
            Assert.IsTrue(Directory.Exists(_testDirectory));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UploadFile_ShouldThrowException_WhenFileStreamIsNull()
        {
            _fileUploadService.UploadFile(null, "test.zip");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UploadFile_ShouldThrowException_WhenFileNameIsNull()
        {
            using (var stream = new MemoryStream())
            {
                _fileUploadService.UploadFile(stream, null);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UploadFile_ShouldThrowException_WhenFileExtensionIsNotAllowed()
        {
            using (var stream = new MemoryStream())
            {
                _fileUploadService.UploadFile(stream, "test.txt");
            }
        }

        [TestMethod]
        public void UploadFile_ShouldSaveFile_WhenValidInput()
        {
            var fileName = "test.zip";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            using (var stream = new MemoryStream(fileContent))
            {
                _fileUploadService.UploadFile(stream, fileName);
            }

            var filePath = Path.Combine(_testDirectory, fileName);
            Assert.IsTrue(File.Exists(filePath));
            CollectionAssert.AreEqual(fileContent, File.ReadAllBytes(filePath));
        }

        [TestMethod]
        public void UploadFile_ShouldOverwriteFile_WhenFileAlreadyExists()
        {
            var fileName = "test.zip";
            var initialContent = new byte[] { 1, 2, 3, 4, 5 };
            var newContent = new byte[] { 6, 7, 8, 9, 0 };

            var filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllBytes(filePath, initialContent);

            using (var stream = new MemoryStream(newContent))
            {
                _fileUploadService.UploadFile(stream, fileName);
            }

            Assert.IsTrue(File.Exists(filePath));
            CollectionAssert.AreEqual(newContent, File.ReadAllBytes(filePath));
        }
    }
}
