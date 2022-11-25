﻿namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Policy;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;
    using log4net;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class TestControllerService : ITestControllerService, IDisposable
    {
        private const string HostUrl = "http://localhost:8087/";

        /// <summary>
        ///     Logger instance
        /// </summary>
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ITestControllerService) };

        public void Initialize()
        {
            try
            {
                Log("Initializing...");
                Task.Run(() => { StartWebApiEndpoints(); }).ConfigureAwait(false);
                Log("Endpoints available");
                Log("Initialized");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }            
        }

        private void StartWebApiEndpoints()
        {
            var webApiBuilder = WebApplication.CreateBuilder();
            webApiBuilder.WebHost.ConfigureServices(services => { services.AddControllers(); });
            webApiBuilder.WebHost.UseUrls($"{HostUrl}")
            .ConfigureKestrel((context, option) =>
            {
                option.AllowSynchronousIO = true;
            });

            var app = webApiBuilder.Build();
            app.MapGet("/healthcheck", (Func<object>)(() => new { Value = "Hello World!" }));
            app.MapControllers();
            app.Run();
        }

        private void Log(string msg)
        {
            _logger.Info(msg);
        }
    }
}