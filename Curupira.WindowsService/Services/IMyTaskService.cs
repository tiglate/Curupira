using Curupira.WindowsService.Model;
using System.Collections.Generic;

namespace Curupira.WindowsService.Service
{
    public interface IMyTaskService
    {
        IEnumerable<TaskModel> GetAllTasks();
    }
}
