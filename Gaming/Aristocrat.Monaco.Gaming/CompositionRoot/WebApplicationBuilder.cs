namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using Microsoft.Extensions.DependencyInjection;

    using CoreWCF.Configuration;
    using CoreWCF.Description;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Aristocrat.Monaco.Common.Communications;
    using System.Threading.Tasks;
    using System;
    using CoreWCF;

    /// <summary>
    /// Responsible for creating and configuring WebApplication object.
    /// </summary>
    public static class WebApplicationBuilder
    {
        /// <summary>
        /// Create a web application object
        /// </summary>
        public static WebApplication GetWcfApplicationRuntime()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel((context, option) =>
            {
                option.AllowSynchronousIO = true;
            });

            // Add WSDL support
            builder.Services.AddServiceModelServices().AddServiceModelMetadata();
            builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

            var app = builder.Build();

            var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;

            return app;
        }
    }

    public sealed class AspNetCoreWebRuntime : IWcfApplicationRuntime
    {
        private readonly WebApplication _app;

        public CommunicationState State { get; private set; }

        public AspNetCoreWebRuntime()
        {
            _app = WebApplicationBuilder.GetWcfApplicationRuntime();
            State = CommunicationState.Created;
        }

        public ValueTask DisposeAsync()
        {
            return _app.DisposeAsync();
        }

        public void Start()
        {
            _app.StartAsync().GetAwaiter().GetResult();
            State = CommunicationState.Opened;
        }

        public Task StopAsync()
        {
            var rst = _app.StopAsync();
            State = CommunicationState.Closed;
            return rst;
        }

        public void UseServiceModel(Action<IServiceBuilder> builder)
        {            
            _app.UseServiceModel(builder);
        }

        public T GetRequiredService<T>() where T : notnull
        {
            return _app.Services.GetRequiredService<T>();
        }
    }
}
