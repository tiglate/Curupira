using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;
using Curupira.Plugins.Contract;

namespace Curupira.WindowsService.Infra
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogProvider _logger;

        public GlobalExceptionFilter(ILogProvider logger)
        {
            _logger = logger;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            _logger.Error(context.Exception, "Unhandled exception occurred.");

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
                new { message = "An error occurred. Please try again later." });
        }
    }
}
