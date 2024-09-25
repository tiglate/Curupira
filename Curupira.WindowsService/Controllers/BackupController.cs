using Curupira.WindowsService.Attributes;
using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/backup")]
    public class BackupController : ApiController
    {
        private readonly IPluginExecutorService _pluginExecutorService;

        public BackupController(IPluginExecutorService pluginExecutorService)
        {
            _pluginExecutorService = pluginExecutorService;
        }

        [HttpGet]
        [Route("run/{archiveId}")]
        public async Task<IHttpActionResult> RunAsync(string archiveId)
        {
            var result = await _pluginExecutorService.ExecutePluginAsync("BackupPlugin", new Dictionary<string, string>
            {
                { "archive", archiveId }
            }).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet]
        [Route("runAll")]
        public async Task<IHttpActionResult> RunAllAsync()
        {
            var result = await _pluginExecutorService.ExecutePluginAsync("BackupPlugin", new Dictionary<string, string>()).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
