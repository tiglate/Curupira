using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.Linq;
using System;

namespace Curupira.WindowsService.Attributes
{
    public class ApiKeyAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly string _apiKey;

        public ApiKeyAuthorizeAttribute()
        {
            _apiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Please define a API_KEY environment variable or create a .env file in dev mode");
            }
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Contains(ApiKeyHeaderName))
            {
                var apiKey = actionContext.Request.Headers.GetValues(ApiKeyHeaderName).FirstOrDefault();
                if (apiKey == _apiKey)
                {
                    // API key is valid, proceed with request
                    return;
                }
            }

            // Invalid API key, reject the request
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid API key");
        }
    }
}
