// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Log4Net.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Aristocrat.Monaco;
using SimpleInjector;
using SimpleInjector.Lifestyles;

var container = new Container();

container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

var binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
Debug.Assert(binPath != null);

var assemblies = Directory.GetFiles(binPath, "Aristocrat.Monaco.Gaming.*.dll")
    .Select(
        file => AssemblyLoadContext.Default
            .LoadFromAssemblyPath(file))
    .ToList();

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(config => { })
    .ConfigureAppConfiguration((context, config) => config
        .SetBasePath(binPath))
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddSimpleInjector(container, options =>
        {
            options.DisposeContainerWithServiceProvider = false;

            // options.AddHostedService<MyHostedService>();

            options.AddLocalization();

            var s = new ServiceCollection();

            s.AddSingleton<Lobby>();

            foreach (var d in s)
            {
                var lifetime = d.Lifetime switch
                {
                    ServiceLifetime.Singleton => Lifestyle.Singleton,
                    ServiceLifetime.Scoped => Lifestyle.Scoped,
                    _ => Lifestyle.Transient
                };

                if (d.ImplementationType != null)
                {
                    container.Register(d.ServiceType, d.ImplementationType, lifetime);
                }
                else
                {
                    container.Register(d.ServiceType, () => d.ImplementationInstance, lifetime);
                }
            }
        });
    })
    .ConfigureLogging((context, config) => config
        .AddLog4Net())
    .UseConsoleLifetime()
    .Build()
    .UseSimpleInjector(container);

container.RegisterInstance<IPropertiesManager>(new PropertiesManager());
// container.RegisterSingleton<Lobby>();

container.Verify();

// var lobby = host.Services.GetRequiredService<Lobby>();
var lobby = container.GetInstance<Lobby>();

host.RunAsync();

Console.WriteLine("Press any key to exit...");

Console.ReadKey();
