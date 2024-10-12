using Curupira.WindowsService.Services;
using Curupira.WindowsService.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Curupira.WindowsService.Tests.Services
{
    [TestClass]
    public class WindowsTasksServiceTests
    {
        private WindowsTasksService _service;
        private ITaskServiceWrapper _taskServiceWrapper;

        [TestInitialize]
        public void Setup()
        {
            _taskServiceWrapper = new TaskServiceWrapper();
            _service = new WindowsTasksService(_taskServiceWrapper);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _taskServiceWrapper?.Dispose();
        }

        [TestMethod]
        public void GetAllTasks_ReturnsTasks()
        {
            // Act
            var result = _service.GetAllTasks().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void GetAllTasks_ExcludesHiddenTasks()
        {
            // Act
            var result = _service.GetAllTasks().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TrueForAll(task => !task.Hidden));
        }
    }
}