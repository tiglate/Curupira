using Curupira.WindowsService.Controllers;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Curupira.Tests.WindowsService.Controllers
{
    [TestClass]
    public class WindowsServicesControllerTests
    {
        private Mock<IWindowsServicesService> _mockServiceService;
        private WindowsServicesController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockServiceService = new Mock<IWindowsServicesService>();
            _controller = new WindowsServicesController(_mockServiceService.Object);
        }

        [TestMethod]
        public void GetAll_ReturnsAllServices()
        {
            // Arrange
            var expectedServices = new List<ServiceModel>
            {
                new ServiceModel { ServiceName = "Service1", Description = "Description1", Status = "Running", StartType = "Automatic", ServiceAccount = "LocalSystem" },
                new ServiceModel { ServiceName = "Service2", Description = "Description2", Status = "Stopped", StartType = "Manual", ServiceAccount = "NetworkService" }
            };
            _mockServiceService.Setup(service => service.GetAllServices()).Returns(expectedServices);

            // Act
            var result = _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedServices.Count, result.Count());
            CollectionAssert.AreEqual(expectedServices, result.ToList());
        }
    }
}
