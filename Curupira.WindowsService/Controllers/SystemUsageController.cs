using System.Web.Http;
using Curupira.WindowsService.Service;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Attributes;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/system")]
    public class SystemUsageController : ApiController
    {
        private readonly ISystemUsageService _systemUsageService;

        public SystemUsageController(ISystemUsageService systemUsageService)
        {
            _systemUsageService = systemUsageService;
        }

        [HttpGet]
        [Route("usage")]
        public SystemUsageModel GetSystemUsage()
        {
            return _systemUsageService.GetSystemUsage();
        }
    }
}
