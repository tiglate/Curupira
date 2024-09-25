using System.Collections.Generic;
using System.Web.Http;
using Curupira.WindowsService.Service;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Attributes;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/eventLog")]
    public class EventLogsController : ApiController
    {
        private readonly IEventLogService _eventLogService;

        public EventLogsController(IEventLogService eventLogService)
        {
            _eventLogService = eventLogService;
        }

        [HttpGet]
        [Route("getAll")]
        public IEnumerable<EventLogModel> GetAll()
        {
            return _eventLogService.GetLatestApplicationLogs();
        }
    }
}
