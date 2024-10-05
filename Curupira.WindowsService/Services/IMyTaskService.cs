using Curupira.WindowsService.Model;
using System.Collections.Generic;

namespace Curupira.WindowsService.Services
{
    public interface IMyTaskService
    {
        IEnumerable<TaskModel> GetAllTasks();
    }
}
