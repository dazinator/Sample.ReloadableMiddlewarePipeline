using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Server
{
    public static class StartupExtensions
    {
        public static IServiceCollection ConfigureReloadablePipeline<TOptions>(this IServiceCollection services,
            IConfiguration configuration, Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
            where TOptions : class
        {
            services.Configure<TOptions>(configuration);
            services.AddSingleton<RequestDelegateFactory<TOptions>>(sp =>
            {
                //ActivatorUtilities.CreateInstance<RequestDelegateFactory<TOptions>>(sp, configure);
                var environment = sp.GetRequiredService<IWebHostEnvironment>();
                var options = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
                return new RequestDelegateFactory<TOptions>(environment, options, configure);
            });
            return services;
        }
        public static IApplicationBuilder UseReloadablePipeline<TOptions>(this IApplicationBuilder builder)
        where TOptions : class
        {
            var isTerminal = false;
            builder.UseMiddleware<ReloadPipelineMiddleware<TOptions>>(builder, isTerminal);
            return builder;
        }

        public static IApplicationBuilder RunReloadablePipeline<TOptions>(this IApplicationBuilder builder)
      where TOptions : class
        {
            var isTerminal = true;
            builder.UseMiddleware<ReloadPipelineMiddleware<TOptions>>(builder, isTerminal);
            return builder;
        }
    }

}
