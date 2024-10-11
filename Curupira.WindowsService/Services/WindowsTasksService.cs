using System;
using System.Collections.Generic;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Wrappers;

namespace Curupira.WindowsService.Services
{
    public class WindowsTasksService : IWindowsTasksService
    {
        private readonly ITaskServiceWrapper _taskService;

        public WindowsTasksService(ITaskServiceWrapper taskService)
        {
            _taskService = taskService;
        }

        public IEnumerable<TaskModel> GetAllTasks()
        {
            var resultList = new List<TaskModel>();

            var rootFolder = _taskService.RootFolder;

            foreach (var task in rootFolder.Tasks)
            {
                if (!task.Definition.Settings.Hidden)
                {
                    var taskModel = new TaskModel
                    {
                        Name = task.Name,
                        Hidden = task.Definition.Settings.Hidden,
                        Status = task.State.ToString(),
                        LastRunTime = task.LastRunTime != DateTime.MinValue ? task.LastRunTime.ToString("g") : "Never",
                        NextRunTime = task.NextRunTime != DateTime.MinValue ? task.NextRunTime.ToString("g") : "Not scheduled"
                    };

                    resultList.Add(taskModel);
                }
            }

            return resultList;
        }
    }
}