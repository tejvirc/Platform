namespace Aristocrat.Monaco.WebApiServer
{
    using Kernel;
    using log4net;
    using Microsoft.AspNetCore.Hosting;
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;

    /// <summary>
    /// Self-hosting server to host all the dynamically loaded APIs.
    /// </summary>
    public class WebApiServerService : IService
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(WebApiServerService) };

        public void Initialize()
        {
            _logger.Info("Initialize 'WebApiServerService'");

            var baseAddress = $"http://{GetLocalIPAddress()}:9099/";

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(baseAddress)
                .UseStartup<Startup>()
                .Build();

            Task.Run(() => { host.Run(); }).ConfigureAwait(false);

            _logger.Info($"Web API Server has started at {baseAddress}");
        }

        private static string GetLocalIPAddress()
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ipAddress?.ToString();
        }
    }
}
