// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using SI;
using SimpleInjector;
using SimpleInjector.Lifestyles;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();

var container = new Container();

container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

services.AddSimpleInjector(container, options =>
{
    options.Container
        .RegisterSingleton<IPropertiesManager, PropertiesManager>();
});

var serviceProvider = services
    .BuildServiceProvider(validateScopes: true)
    .UseSimpleInjector(container);

// container.RegisterInstance<IPropertiesManager>(new PropertiesManager());
// container.RegisterSingleton<IPropertiesManager, PropertiesManager>();
// container.RegisterSingleton<Lobby>();

container.Verify();

var properties = serviceProvider.GetRequiredService<IPropertiesManager>();

Console.WriteLine("Press any key to exit...");

Console.ReadKey();
