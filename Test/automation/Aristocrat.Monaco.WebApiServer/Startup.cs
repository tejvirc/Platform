namespace Aristocrat.Monaco.WebApiServer
{
    using log4net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class Startup
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();// local controller was loaded here

            var controllerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoadControllers");
            if (Directory.Exists(controllerPath))
            {
                var controllerAssemblies = Directory.GetFiles(controllerPath, "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom)
                    .ToList();

                try
                {
                    foreach (var loadedAssembly in controllerAssemblies)
                    {
                        var type = loadedAssembly.GetType($"{loadedAssembly.FullName.Split(',')[0]}.Setup");
                        if (type != null)
                        {
                            var methodInfo = type.GetMethod("InitialSetup");
                            if (methodInfo != null)
                            {
                                _logger.Info($"WebApiServer: InitialSetup function invoked for : '{loadedAssembly.FullName}'");
                                methodInfo.Invoke(null, null);
                            }
                        }
                    }

                    _logger.Info($"WebApiServer: Number of Dll Controllers loaded : '{controllerAssemblies.Count}'");

                    // Inject loaded controllers
                    services.AddControllers().ConfigureApplicationPartManager(apm =>
                    {
                        foreach (var controllerAssembly in controllerAssemblies)
                        {
                            apm.ApplicationParts.Add(new AssemblyPart(controllerAssembly));
                        }
                    }).AddControllersAsServices();
                }
                catch (Exception e)
                {
                    _logger.Error($"WebApiServer:Error occured while loading API DLLs : '{e.Message}'");
                }
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            _logger.Info("WebApiServer: Endpoint added.");
        }
    }
}
