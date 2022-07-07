using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Threading.Tasks;

namespace api
{
    public class SampleFunc
    {
        private readonly ILogger<SampleFunc> _logger;
        private readonly IConfiguration _configuration;
        // The web API will only accept tokens 1) for users, and 2) having the "access_as_user" scope for this API	
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };
        public SampleFunc(IConfiguration configuration, ILogger<SampleFunc> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [FunctionName("SampleFunc")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            _logger.LogInformation(_configuration["AzureAd:ClientId"]);

            var (authenticationStatus, authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();
            if (!authenticationStatus) return authenticationResponse;

            req.HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            string name = req.HttpContext.User.Identity.IsAuthenticated ? req.HttpContext.User.GetDisplayName() : null;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new JsonResult(responseMessage);
        }

    }
}
