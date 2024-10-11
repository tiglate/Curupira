using Curupira.WindowsService.Model;
using System.Collections.Generic;

namespace Curupira.WindowsService.Services
{
    public interface IWindowsTasksService
    {
        IEnumerable<TaskModel> GetAllTasks();
    }
}
