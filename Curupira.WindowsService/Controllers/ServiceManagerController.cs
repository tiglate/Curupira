using Curupira.WindowsService.Attributes;
using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/serviceManager")]
    public class ServiceManagerController : ApiController
    {
        private readonly IPluginExecutorService _pluginExecutorService;

        public ServiceManagerController(IPluginExecutorService pluginExecutorService)
        {
            _pluginExecutorService = pluginExecutorService;
        }

        [HttpGet]
        [Route("run/{bundleId}")]
        public async Task<IHttpActionResult> RunAsync(string bundleId)
        {
            var result = await _pluginExecutorService.ExecutePluginAsync("ServiceManagerPlugin", new Dictionary<string, string>
            {
                { "bundle", bundleId }
            }).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
