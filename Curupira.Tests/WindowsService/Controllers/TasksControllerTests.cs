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
    public class TasksControllerTests
    {
        private Mock<IWindowsTasksService> _mockTaskService;
        private TasksController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTaskService = new Mock<IWindowsTasksService>();
            _controller = new TasksController(_mockTaskService.Object);
        }

        [TestMethod]
        public void GetAll_ReturnsAllTasks()
        {
            // Arrange
            var expectedTasks = new List<TaskModel>
            {
                new TaskModel { Name = "Task1", Status = "Running", LastRunTime = "2023-01-01T00:00:00", NextRunTime = "2023-01-02T00:00:00", Hidden = false },
                new TaskModel { Name = "Task2", Status = "Completed", LastRunTime = "2023-01-01T01:00:00", NextRunTime = "2023-01-02T01:00:00", Hidden = true }
            };
            _mockTaskService.Setup(service => service.GetAllTasks()).Returns(expectedTasks);

            // Act
            var result = _controller.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedTasks.Count, result.Count());
            CollectionAssert.AreEqual(expectedTasks, result.ToList());
        }
    }
}
