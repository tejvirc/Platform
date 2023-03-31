namespace Aristocrat.Monaco.WebApiServer
{
    using Kernel;
    using log4net;
    using Microsoft.AspNetCore.Hosting;
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Self-hosting server to host all the dynamically loaded APIs.
    /// </summary>
    public class WebApiServerService : IService
    {
        private const string BaseAddress = "http://localhost:9099/";

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(WebApiServerService) };

        public void Initialize()
        {
            _logger.Info("Initialize 'WebApiServerService'");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(BaseAddress)
                .UseStartup<Startup>()
                .Build();

            Task.Run(() => { host.Run(); }).ConfigureAwait(false);

            _logger.Info($"Web API Server has started at {BaseAddress}");
        }
    }
}
