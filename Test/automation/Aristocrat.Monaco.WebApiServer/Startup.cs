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
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();// local controller was loaded here

            var controllerPath = Path.Combine(AppContext.BaseDirectory, "LoadControllers");
            if (!Directory.Exists(controllerPath))
            {
                return;
            }

            var controllerAssemblies = Directory.GetFiles(controllerPath, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();

            try
            {
                foreach (var loadedAssembly in controllerAssemblies)
                {
                    var type = loadedAssembly.GetType($"{loadedAssembly.FullName?.Split(',')[0]}.Setup");
                    var methodInfo = type?.GetMethod("InitialSetup");
                    if (methodInfo is null)
                    {
                        continue;
                    }

                    _logger.Info($"WebApiServer: InitialSetup function invoked for : '{loadedAssembly.FullName}'");
                    methodInfo.Invoke(null, null);
                }

                _logger.Info($"WebApiServer: Number of Dll Controllers loaded : '{controllerAssemblies.Count}'");

                // Inject loaded controllers
                services.AddControllers().ConfigureApplicationPartManager(
                    apm =>
                    {
                        foreach (var controllerAssembly in controllerAssemblies)
                        {
                            apm.ApplicationParts.Add(new AssemblyPart(controllerAssembly));
                        }
                    }).AddControllersAsServices().AddJsonOptions(
                    options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = null;
                        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    });
            }
            catch (Exception e)
            {
                _logger.Error($"WebApiServer:Error occurred while loading API DLLs : '{e.Message}'");
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
