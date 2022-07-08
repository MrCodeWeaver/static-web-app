using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Azure.Functions.Identity.Web.Extensions;

[assembly: FunctionsStartup(typeof(static_web_app.Startup))]

namespace static_web_app
{

    public class Startup : FunctionsStartup
    {
        private readonly ILogger<Startup> _logger;

        public Startup( ILogger<Startup> logger)
        {
            _logger = logger;

        }

        IConfiguration Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the azure function application directory. 'C:\whatever' for local and 'd:\home\whatever' for Azure
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            var currentDirectory = executionContextOptions.AppDirectory;

            // Get the original configuration provider from the Azure Function
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            // Create a new IConfigurationRoot and add our configuration along with Azure's original configuration 
            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddConfiguration(configuration) // Add the original function configuration 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            _logger.LogInformation("Startup Configure");
            _logger.LogInformation(Configuration.ToString());

            // Replace the Azure Function configuration with our new one
            builder.Services.AddSingleton(Configuration);
            // builder.AddCors("AllowAnyOrigin");
           // builder.UseCors("AllowAnyOrigin");


            ConfigureServices(builder.Services);

        }

        private void ConfigureServices(IServiceCollection services)
        {

            _logger.LogInformation("Startup ConfigureServices");
          //  _logger.LogInformation(_configuration["AzureAd"]);
            //services.AddAuthentication(sharedOptions =>
            //{
            //    sharedOptions.DefaultScheme = Microsoft.Identity.Web.Constants.Bearer;
            //    sharedOptions.DefaultChallengeScheme = Microsoft.Identity.Web.Constants.Bearer;
            //})
            //    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Constants.Bearer;
                sharedOptions.DefaultChallengeScheme = Constants.Bearer;
            })
            .AddArmToken()
            .AddScriptAuthLevel()
            .AddMicrosoftIdentityWebApi(Configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin",
                    builder =>
                    {
                        builder.SetIsOriginAllowed(origin => true);
                        builder.AllowCredentials();
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    }
                );
            });
            //services.UseCors

                services.AddAuthorization(options => options.AddScriptPolicies());

            services
                .AddAuthLevelAuthorizationHandler()
                .AddNamedAuthLevelAuthorizationHandler()
                .AddFunctionAuthorizationHandler();
        }
    }
}
