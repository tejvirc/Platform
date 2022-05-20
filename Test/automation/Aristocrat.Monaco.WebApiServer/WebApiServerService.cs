using System;
using System.Collections.Generic;
namespace Aristocrat.Monaco.WebApiServer
{
    using Kernel;
    using log4net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;
    using System.Web.Http.SelfHost;

    /// <summary>
    /// Self-hosting server to host all the dynamically loaded APIs.
    /// </summary>
    public class WebApiServerService : IService
    {
        private const string BaseAddress = "http://localhost:9099/";

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private HttpSelfHostConfiguration _config;

        private HttpSelfHostServer _server;

        private Task _serverTask;
        public string Name => GetType().ToString();
        public ICollection<Type> ServiceTypes => new[] { typeof(WebApiServerService) };

        public void Initialize()
        {
            _logger.Info("Initialize 'WebApiServerService'");

            //Configure and start server
            ConfigureAndSetupServer();

            // This will force all assemblies to load at Monaco booting and let them run their init code.
            // Assembly resolver is called only once. Either it can be called forcefully now, or when the first API is hit.
            ForceAssembliesToLoad();

            _logger.Info($"Web API Server has started at {BaseAddress}");
        }

        private void ConfigureAndSetupServer()
        {
            _config = new HttpSelfHostConfiguration(BaseAddress);
            _config.Services.Replace(typeof(IAssembliesResolver), new AssembliesResolver());

            _config.Routes.MapHttpRoute("Default",
                                         "Platform/{controller}/{action}/{id}",
                                         new { id = RouteParameter.Optional });
            _config.Routes.MapHttpRoute("DefaultApiGet", "Platform/{controller}",
                new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            _config.Routes.MapHttpRoute("DefaultApiPost", "Platform/{controller}",
                new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            _server = new HttpSelfHostServer(_config);
            _serverTask = _server.OpenAsync();
            _serverTask.Wait();
        }

        private void ForceAssembliesToLoad()
        {
            //Create HttpClient and make a request to dummy api.
            // TODO: couldn't find a better way to force all dlls to load by AssemblyResolver.
            HttpClient client = new HttpClient();
            var _ = client.GetAsync(BaseAddress + "Platform/LoadAllDLLsController/Load").Result;
        }
    }
}
