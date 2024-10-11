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
    public class InstallerControllerTests
    {
        private Mock<IPluginExecutorService> _mockPluginExecutorService;
        private InstallerController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPluginExecutorService = new Mock<IPluginExecutorService>();
            _controller = new InstallerController(_mockPluginExecutorService.Object);
        }

        [TestMethod]
        public async Task RunAsync_ReturnsOkResult()
        {
            // Arrange
            var componentId = "testComponentId";
            _mockPluginExecutorService
                .Setup(service => service.ExecutePluginAsync("InstallerPlugin", It.Is<Dictionary<string, string>>(d => d["component"] == componentId)))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RunAsync(componentId) as OkNegotiatedContentResult<bool>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content);
        }
    }
}
