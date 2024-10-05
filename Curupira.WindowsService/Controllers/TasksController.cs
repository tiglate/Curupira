using System.Collections.Generic;
using System.Web.Http;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using Curupira.WindowsService.Attributes;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/tasks")]
    public class TasksController : ApiController
    {
        private readonly IMyTaskService _taskService;

        public TasksController(IMyTaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [Route("getAll")]
        public IEnumerable<TaskModel> GetAll()
        {
            return _taskService.GetAllTasks();
        }
    }
}
