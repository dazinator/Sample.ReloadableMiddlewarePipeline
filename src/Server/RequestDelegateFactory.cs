using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Server
{
    public class RequestDelegateFactory<TOptions> : IDisposable
        where TOptions : class
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IOptionsMonitor<TOptions> _optionsMonitor;
        private readonly Action<IApplicationBuilder, IWebHostEnvironment, TOptions> _configure;
        private RequestDelegate _currentRequestDelegate = null;
        private readonly object _currentInstanceLock = new object();
        private readonly IDisposable _listening = null;

        public IApplicationBuilder _subBuilder { get; set; }

        public RequestDelegateFactory(IWebHostEnvironment environment,
            IOptionsMonitor<TOptions> optionsMonitor,
            Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
        {
            _environment = environment;
            _optionsMonitor = optionsMonitor;
            _configure = configure;
            _listening = _optionsMonitor.OnChange(Invalidate);
        }

        private void Invalidate(TOptions options, string name)
        {
            _currentRequestDelegate = null; // next call to get will build a new one.
        }

        public RequestDelegate Get(IApplicationBuilder builder, RequestDelegate onNext, bool isTerminal)
        {
            var existing = _currentRequestDelegate;
            if (existing != null)
            {
                return existing;
            }

            lock (_currentInstanceLock)
            {
                if (existing != null)
                {
                    return existing;
                }

                _subBuilder = builder.New();

                _configure(_subBuilder, _environment, _optionsMonitor.CurrentValue);

                // if nothing in this pipeline runs, join back to root pipeline?
                if (!isTerminal && onNext != null)
                {
                    _subBuilder.Run(onNext);
                    //_subBuilder.Run(async (http) => await onNext());
                }
                var newInstance = _subBuilder.Build();
                _currentRequestDelegate = newInstance;


                // as we don't lock in Invalidate(), it could have just set _currentRequestDelegate back to null here,
                // that's why we keep hold of and return, newInstance - as this method must always return an instance to satisfy current request.
                return newInstance;
            }
        }

        public void Dispose() => _listening?.Dispose();
    }
}