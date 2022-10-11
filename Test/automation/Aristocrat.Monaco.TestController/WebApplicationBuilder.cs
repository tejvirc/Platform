namespace Aristocrat.Monaco.TestController
{
    using Microsoft.Extensions.DependencyInjection;

    using CoreWCF.Configuration;
    using CoreWCF.Description;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using System;

    /// <summary>
    /// Responsible for creating and configuring WebApplication object.
    /// </summary>
    public static class WebApplicationBuilder
    {
        /// <summary>
        /// Create a web application object
        /// </summary>
        public static WebApplication GetWcfApplicationRuntime(Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
        {
            var app = builder.Build();

            var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;

            return app;
        }

        public static Microsoft.AspNetCore.Builder.WebApplicationBuilder GetSOAPWebApplicationBuilder()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.ConfigureKestrel((context, option) =>
            {
                option.AllowSynchronousIO = true;
            });

            // Add WSDL support
            builder.Services.AddServiceModelServices().AddServiceModelMetadata();
            builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

            return builder;
        }


        public static Microsoft.AspNetCore.Builder.WebApplicationBuilder GetRESTWebApplicationBuilder()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.ConfigureKestrel((context, option) =>
            {
                option.AllowSynchronousIO = true;
            });

            builder.Services.AddServiceModelWebServices();

            return builder;
        }
    }
}
