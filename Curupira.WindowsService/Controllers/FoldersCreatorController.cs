using Curupira.WindowsService.Attributes;
using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/foldersCreator")]
    public class FoldersCreatorController : ApiController
    {
        private readonly IPluginExecutorService _pluginExecutorService;

        public FoldersCreatorController(IPluginExecutorService pluginExecutorService)
        {
            _pluginExecutorService = pluginExecutorService;
        }

        [HttpGet]
        [Route("run")]
        public async Task<IHttpActionResult> RunAsync()
        {
            var result = await _pluginExecutorService.ExecutePluginAsync("FoldersCreatorPlugin", new Dictionary<string, string>()).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
