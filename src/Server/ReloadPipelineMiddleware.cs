using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Server
{
    public class ReloadPipelineMiddleware<TOptions>
       where TOptions : class
    {
        private readonly RequestDelegate _next;
        private readonly RequestDelegateFactory<TOptions> _factory;
        private readonly IApplicationBuilder _rootBuilder;
        private readonly bool _isTerminal;

        public ReloadPipelineMiddleware(
            RequestDelegate next,            
            IApplicationBuilder rootBuilder, 
            RequestDelegateFactory<TOptions> factory, bool isTerminal)
        {
            _next = next;
            _factory = factory;
            _rootBuilder = rootBuilder;
            _isTerminal = isTerminal;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestDelegate = _factory.Get(_rootBuilder, _next, _isTerminal);
            await requestDelegate.Invoke(context);
        }

    }
}