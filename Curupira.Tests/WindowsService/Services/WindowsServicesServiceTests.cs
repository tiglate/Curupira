using Curupira.WindowsService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Curupira.Tests.WindowsService.Services
{
    [TestClass]
    public class WindowsServicesServiceTests
    {
        private WindowsServicesService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new WindowsServicesService();
        }

        [TestMethod]
        public void GetAllServices_ReturnsServices()
        {
            // Act
            var result = _service.GetAllServices().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void GetAllServices_ContainsExpectedService()
        {
            // Act
            var result = _service.GetAllServices().ToList();

            // Assert
            Assert.IsNotNull(result);
            var expectedService = result.Find(s => s.ServiceName == "EventLog");
            Assert.IsNotNull(expectedService);
            Assert.AreEqual("EventLog", expectedService.ServiceName);
        }
    }
}
