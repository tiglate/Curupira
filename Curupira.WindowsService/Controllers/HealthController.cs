using System;
using System.Web.Http;

namespace Curupira.WindowsService.Controllers
{
    public class HealthController : ApiController
    {
        public static bool ShouldThrowException { get; set; } = false;

        [HttpGet]
        [Route("api/health")]
        public IHttpActionResult CheckHealth()
        {
            if (ShouldThrowException)
            {
#pragma warning disable S112 // General or reserved exceptions should never be thrown
                throw new Exception("This is an unhandled exception.");
#pragma warning restore S112 // General or reserved exceptions should never be thrown
            }

            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}
