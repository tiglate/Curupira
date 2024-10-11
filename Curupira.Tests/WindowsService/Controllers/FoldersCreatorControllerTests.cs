using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Curupira.Tests.WindowsService.Controllers
{
    [TestClass]
    public class FoldersCreatorControllerTests
    {
        private Mock<IPluginExecutorService> _mockPluginExecutorService;
        private FoldersCreatorController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPluginExecutorService = new Mock<IPluginExecutorService>();
            _controller = new FoldersCreatorController(_mockPluginExecutorService.Object);
        }

        [TestMethod]
        public async Task RunAsync_ReturnsOkResult()
        {
            // Arrange
            _mockPluginExecutorService
                .Setup(service => service.ExecutePluginAsync("FoldersCreatorPlugin", It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RunAsync() as OkNegotiatedContentResult<bool>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content);
        }
    }
}
