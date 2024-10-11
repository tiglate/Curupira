using Curupira.WindowsService.Attributes;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/services")]
    public class WindowsServicesController : ApiController
    {
        private readonly IWindowsServicesService _serviceService;

        public WindowsServicesController(IWindowsServicesService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        [Route("getAll")]
        public IEnumerable<ServiceModel> GetAll()
        {
            return _serviceService.GetAllServices();
        }
    }
}
