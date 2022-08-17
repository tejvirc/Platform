namespace Aristocrat.Monaco.WebApiServer
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;
    using System.Web.Http.SelfHost;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Self-hosting server to host all the dynamically loaded APIs.
    /// </summary>
    public class WebApiServerService : IService, IDisposable
    {
        private const string BaseAddress = "http://localhost:9099/";

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private HttpSelfHostConfiguration _config;

        private HttpSelfHostServer _server;

        private Task _serverTask;
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(WebApiServerService) };

        public void Initialize()
        {
            _logger.Info("Initialize 'WebApiServerService'");

            // Configure and start server
            ConfigureAndSetupServer();

            // This will force all assemblies to load at Monaco booting and let them run their init code.
            // Assembly resolver is called only once. Either it can be called forcefully now, or when the first API is hit.
            ForceAssembliesToLoad();

            _logger.Info($"Web API Server has started at {BaseAddress}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Note: Do not use null conditional operator below. Causes a false positive in Code Analysis.
                // https://github.com/dotnet/roslyn-analyzers/issues/291
                if (_config != null)
                {
                    _config.Dispose();
                }
                if (_server != null)
                {
                    _server.Dispose();
                }
                if (_serverTask != null)
                {
                    _serverTask.Dispose();
                }
            }

            _disposed = true;
        }

        private void ConfigureAndSetupServer()
        {
            _config = new HttpSelfHostConfiguration(BaseAddress);
            _config.Services.Replace(typeof(IAssembliesResolver), new AssembliesResolver());

            _config.Routes.MapHttpRoute(
                "Default",
                "Platform/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional });
            _config.Routes.MapHttpRoute(
                "DefaultApiGet",
                "Platform/{controller}",
                new { action = "Get" },
                new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            _config.Routes.MapHttpRoute(
                "DefaultApiPost",
                "Platform/{controller}",
                new { action = "Post" },
                new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            _server = new HttpSelfHostServer(_config);
            _serverTask = _server.OpenAsync();
            _serverTask.Wait();
        }

        private void ForceAssembliesToLoad()
        {
            // Create HttpClient and make a request to dummy api.
            // There is probably a better way to force all DLLs to load by AssemblyResolver.
            var client = new HttpClient();
            _ = client.GetAsync(BaseAddress + "Platform/LoadAllDLLsController/Load").Result;
        }
    }
}