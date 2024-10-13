using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Services;

namespace Curupira.WindowsService.Tests.Controllers
{
    [TestClass]
    public class FileUploadControllerTests
    {
        private Mock<IFileUploadService> _mockFileUploadService;
        private FileUploadController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
            _controller = new FileUploadController(_mockFileUploadService.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        [TestMethod]
        public async Task UploadFile_ShouldReturnBadRequest_WhenContentIsNotMultipart()
        {
            // Arrange
            _controller.Request.Content = new StringContent("Invalid content");

            // Act
            var result = await _controller.UploadFile();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            var badRequestResult = result as BadRequestErrorMessageResult;
            Assert.AreEqual("Unsupported media type.", badRequestResult.Message);
        }

        [TestMethod]
        public async Task UploadFile_ShouldReturnOk_WhenFileIsUploadedSuccessfully()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "test.zip"
            };
            content.Add(fileContent);

            _controller.Request.Content = content;

            // Act
            var result = await _controller.UploadFile();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<string>));
            var okResult = result as OkNegotiatedContentResult<string>;
            Assert.AreEqual("File uploaded successfully.", okResult.Content);
        }

        [TestMethod]
        public async Task UploadFile_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "test.zip"
            };
            content.Add(fileContent);

            _controller.Request.Content = content;

            _mockFileUploadService.Setup(s => s.UploadFile(It.IsAny<Stream>(), It.IsAny<string>()))
                                  .Throws(new Exception("Test exception"));

            // Act
            var result = await _controller.UploadFile();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            var badRequestResult = result as BadRequestErrorMessageResult;
            Assert.AreEqual("Error uploading file: Test exception", badRequestResult.Message);
        }
    }
}
