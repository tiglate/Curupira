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
    public class ServiceManagerControllerTests
    {
        private Mock<IPluginExecutorService> _mockPluginExecutorService;
        private ServiceManagerController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockPluginExecutorService = new Mock<IPluginExecutorService>();
            _controller = new ServiceManagerController(_mockPluginExecutorService.Object);
        }

        [TestMethod]
        public async Task RunAsync_ReturnsOkResult()
        {
            // Arrange
            var bundleId = "testBundleId";
            _mockPluginExecutorService
                .Setup(service => service.ExecutePluginAsync("ServiceManagerPlugin", It.Is<Dictionary<string, string>>(d => d["bundle"] == bundleId)))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RunAsync(bundleId) as OkNegotiatedContentResult<bool>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content);
        }
    }
}