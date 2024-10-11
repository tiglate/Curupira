using Microsoft.Win32.TaskScheduler;
using System;

namespace Curupira.WindowsService.Wrappers
{
    public class TaskServiceWrapper : ITaskServiceWrapper
    {
        private readonly TaskService _taskService;

        public TaskServiceWrapper()
        {
            _taskService = new TaskService();
        }

        public TaskFolder RootFolder => _taskService.RootFolder;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _taskService?.Dispose();
            }
        }
    }
}
