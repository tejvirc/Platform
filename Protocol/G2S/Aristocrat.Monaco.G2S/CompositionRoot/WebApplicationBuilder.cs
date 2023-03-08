namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Communications;
    using CoreWCF;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Responsible for creating and configuring WebApplication object.
    /// </summary>
    public static class WebApplicationBuilder
    {
        /// <summary>
        /// Create a web application object
        /// </summary>
        public static WebApplication GetWcfApplicationRuntime(int port, Action<IServiceCollection> serviceBuilder)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel((context, option) =>
            {
                option.ListenAnyIP(port);
                option.AllowSynchronousIO = true;
            });

            // Add WSDL support
            builder.Services.AddServiceModelServices().AddServiceModelMetadata();
            builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
            serviceBuilder?.Invoke(builder.Services);

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

        public AspNetCoreWebRuntime(int port, Action<IServiceCollection> serviceBuilder)
        {
            _app = WebApplicationBuilder.GetWcfApplicationRuntime(port, serviceBuilder);
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
