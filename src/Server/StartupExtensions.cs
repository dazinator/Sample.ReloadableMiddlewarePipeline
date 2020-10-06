using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Server
{
    public static class StartupExtensions
    {        
        public static IApplicationBuilder UseReloadablePipeline<TOptions>(this IApplicationBuilder builder,
            Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
        where TOptions : class
        {
            return AddReloadablePipeline<TOptions>(builder, configure, false);
        }

        public static IApplicationBuilder RunReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
      where TOptions : class
        {
            return AddReloadablePipeline<TOptions>(builder, configure,  true);
        }

        private static IApplicationBuilder AddReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure, bool isTerminal)
    where TOptions : class
        {
            var env = builder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var monitor = builder.ApplicationServices.GetRequiredService<IOptionsMonitor<TOptions>>();

            var factory = new RequestDelegateFactory<TOptions>(env, monitor, configure);

            builder.UseMiddleware<ReloadPipelineMiddleware<TOptions>>(builder, factory, isTerminal);
            return builder;
        }
    }

}
