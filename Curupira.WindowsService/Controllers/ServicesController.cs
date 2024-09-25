using Curupira.WindowsService.Attributes;
using Curupira.WindowsService.Model;
using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/services")]
    public class ServicesController : ApiController
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
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
