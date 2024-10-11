using Curupira.WindowsService.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Curupira.Tests.WindowsService.Controllers
{
    [TestClass]
    public class HealthControllerTests
    {
        private HealthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _controller = new HealthController();
        }

        [TestMethod]
        public void CheckHealth_ReturnsHealthyStatus()
        {
            // Act
            var result = _controller.CheckHealth();

            // Assert
            Assert.IsNotNull(result);

            // Use reflection to access the properties of the anonymous type
            var okResultType = result.GetType();
            var contentProperty = okResultType.GetProperty("Content");
            Assert.IsNotNull(contentProperty);

            var content = contentProperty.GetValue(result);
            var statusProperty = content.GetType().GetProperty("status");
            var timestampProperty = content.GetType().GetProperty("timestamp");

            Assert.IsNotNull(statusProperty);
            Assert.IsNotNull(timestampProperty);

            var status = statusProperty.GetValue(content, null) as string;
            var timestamp = (DateTime)timestampProperty.GetValue(content, null);

            Assert.AreEqual("Healthy", status);
            Assert.IsTrue(timestamp <= DateTime.UtcNow);
        }
    }
}