using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Server
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configSection = Configuration.GetSection("Pipeline");
            services.Configure<PipelineOptions>(configSection);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {            
            // Note: Use vs Run (latter is terminal, former is not)
            // make a change to appsettings.json "Pipelines" section and watch log output in console on furture requests.
            app.UseReloadablePipeline<PipelineOptions>(ConfigureReloadablePipeline);
            app.UseWelcomePage();
        }

        private void ConfigureReloadablePipeline(IApplicationBuilder appBuilder, IWebHostEnvironment environment, PipelineOptions options)
        {
            var logger = appBuilder.ApplicationServices.GetRequiredService<ILogger<Startup>>();
            logger.LogInformation("Building reloadable pipeline from current options!");

            if (options.UseDeveloperExceptionPage)
            {
                appBuilder.Use(async (context, onNext) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Using dev middleware!");
                    await onNext();
                });
            }
            else
            {
                appBuilder.Use(async (context, onNext) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("Not using dev middleware..");
                    await onNext();
                });
            }
        }
    }
}