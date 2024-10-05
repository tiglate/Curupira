using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;
using Curupira.Plugins.Contract;
using System;

namespace Curupira.WindowsService.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogProvider _logger;

        public GlobalExceptionFilterAttribute(ILogProvider logger)
        {
            _logger = logger;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            _logger.Error(actionExecutedContext.Exception, "Unhandled exception occurred.");

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError,
                new { message = "An error occurred. Please try again later." });
        }
    }
}
