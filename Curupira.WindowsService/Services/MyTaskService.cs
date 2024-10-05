using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;
using Curupira.WindowsService.Model;

namespace Curupira.WindowsService.Services
{
    public class MyTaskService : IMyTaskService
    {
        public IEnumerable<TaskModel> GetAllTasks()
        {
            var resultList = new List<TaskModel>();

            // Use Task Scheduler Managed Wrapper to access tasks in the root folder
            using (var taskService = new TaskService())
            {
                // Get the tasks from the root folder
                var rootFolder = taskService.RootFolder;

                foreach (var task in rootFolder.Tasks)
                {
                    // Exclude hidden tasks if necessary
                    if (!task.Definition.Settings.Hidden)
                    {
                        var taskModel = new TaskModel
                        {
                            Name = task.Name,
                            Status = task.State.ToString(),
                            LastRunTime = task.LastRunTime != DateTime.MinValue ? task.LastRunTime.ToString("g") : "Never",
                            NextRunTime = task.NextRunTime != DateTime.MinValue ? task.NextRunTime.ToString("g") : "Not scheduled"
                        };

                        resultList.Add(taskModel);
                    }
                }
            }

            return resultList;
        }
    }
}
