using System;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    public class HealthController : ApiController
    {
        [HttpGet]
        [Route("api/health")]
        public IHttpActionResult CheckHealth()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}
