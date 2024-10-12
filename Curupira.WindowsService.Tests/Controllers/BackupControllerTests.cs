using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Curupira.WindowsService.Tests.Controllers
{
    [TestClass]
    public class BackupControllerTests
    {
        private Mock<IPluginExecutorService> _mockPluginExecutorService;
        private BackupController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPluginExecutorService = new Mock<IPluginExecutorService>();
            _controller = new BackupController(_mockPluginExecutorService.Object);
        }

        [TestMethod]
        public async Task RunAsync_ReturnsOkResult()
        {
            // Arrange
            var archiveId = "testArchiveId";
            _mockPluginExecutorService
                .Setup(service => service.ExecutePluginAsync("BackupPlugin", It.Is<Dictionary<string, string>>(d => d["archive"] == archiveId)))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RunAsync(archiveId) as OkNegotiatedContentResult<bool>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content);
        }

        [TestMethod]
        public async Task RunAllAsync_ReturnsOkResult()
        {
            // Arrange
            _mockPluginExecutorService
                .Setup(service => service.ExecutePluginAsync("BackupPlugin", It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RunAllAsync() as OkNegotiatedContentResult<bool>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content);
        }
    }
}