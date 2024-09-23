﻿using Curupira.WindowsService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    [RoutePrefix("api/v1/installer")]
    public class InstallerController : ApiController
    {
        private readonly IPluginExecutorService _pluginExecutorService;

        public InstallerController(IPluginExecutorService pluginExecutorService)
        {
            _pluginExecutorService = pluginExecutorService;
        }

        [HttpGet]
        [Route("run/{componentId}")]
        public async Task<IHttpActionResult> Run(string componentId)
        {
            var result = await _pluginExecutorService.ExecutePluginAsync("InstallerPlugin", new Dictionary<string, string>
            {
                { "component", componentId }
            });
            return Ok(result);
        }
    }
}