namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Accounting.Contracts;
using Application.Contracts.EdgeLight;
using Application.Contracts.Localization;
using CommandHandlers;
using Commands;
using Common.Container;
using Consumers;
using Contracts;
using Contracts.Lobby;
using Extensions.Fluxor;
using Fluxor;
using Hardware.Contracts.Audio;
using Hardware.Contracts.Cabinet;
using Kernel;
using Log4Net.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monaco.UI.Common;
using Progressives;
using Regions;
using Services;
using Services.Attendant;
using Services.Attract;
using Services.Audio;
using Services.Bank;
using Services.EdgeLighting;
using Services.Translate;
using Services.Upi;
using SimpleInjector;
using SimpleInjector.Packaging;
using Store.Middleware;
using Vgt.Client12.Application.OperatorMenu;

public class LobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        //container.Register<ILobby, LobbyEngine>(Lifestyle.Singleton);

        //container.Register<ILobbyService, LobbyService>(Lifestyle.Singleton);

        //container.Register<IGameLoader, GameLoader>(Lifestyle.Singleton);

        //container.Register<ILayoutManager, LayoutManager>(Lifestyle.Singleton);

        ////container.Register<IOperatorMenuController, OperatorMenuController>(Lifestyle.Singleton);

        //container.Register<IScreenMapper, ScreenMapper>();

        //container.Register<IRegionManager, RegionManager>(Lifestyle.Singleton);

        //container.Register<IRegionViewRegistry, RegionViewRegistry>(Lifestyle.Singleton);

        //container.Register<IRegionNavigator, RegionNavigator>(Lifestyle.Transient);

        //container.Register<DelayedRegionCreationStrategy>(Lifestyle.Transient);

        //container.Register(typeof(IRegionCreator<>), typeof(RegionCreator<>), Lifestyle.Transient);

        ////var regionAdapterMapper = new RegionAdapterMapper(container);
        ////regionAdapterMapper.Register<ContentControlRegionAdapter>(typeof(ContentControl));

        ////container.RegisterInstance<IRegionAdapterMapper>(regionAdapterMapper);

        //container.Register<IApplicationCommands, ApplicationCommands>(Lifestyle.Singleton);

        //container.Register<IObjectFactory, ObjectFactory>(Lifestyle.Singleton);

        //container.Register<ICommandHandlerFactory, CommandHandlerFactory>(Lifestyle.Singleton);
        //container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());

        //container.RegisterConsumers();

        //container.RegisterFluxor();

        //container.RegisterInstance(ServiceManager.GetInstance().GetService<IWpfWindowLauncher>());

        //container.RegisterSingleton(
        //    () => container.GetInstance<IPropertiesManager>()
        //              .GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null) ??
        //          throw new InvalidOperationException("Lobby configuration not found"));

        //((MonacoApplication)Application.Current).Services = container;

        var binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = binPath,
            WebRootPath = "wwwroot",
            Args = Environment.GetCommandLineArgs()
        });

        builder.Services.AddSimpleInjector(container, options =>
        {
            options.DisposeContainerWithServiceProvider = false;

            //options.AddAspNetCore()
            //    .AddControllerActivation()
            //    .AddViewComponentActivation()
            //    .AddPageModelActivation()
            //    .AddTagHelperActivation()
            //    .AddLogging()
            //    .AddLocalization();
        });

        builder.Configuration
            .SetBasePath(binPath)
            //.AddJsonFile("appsettings.json", false, true)
            //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", false, true)
            .AddEnvironmentVariables();

        builder.Host.UseLog4Net();

        // builder.Services.AddLocalization();

        //builder.Services.AddRazorPages();
        //builder.Services.AddServerSideBlazor();
        //builder.Services.AddWpfBlazorWebView();
        builder.Services
            .AddControllersWithViews();

        ConfigureServices(builder.Services);

        // TODO Refactor to use DI container
        ServiceManager.GetInstance().AddService(new SharedConsumerContext());

        container.RegisterManyForOpenGeneric(typeof(IConsumes<>), Assembly.GetExecutingAssembly());

        var app = builder.Build();

        app.Services.UseSimpleInjector(container);

        app.UseStaticFiles();

        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        //app.MapBlazorHub();
        //app.MapFallbackToPage("/_Host");

        ((MonacoApplication)Application.Current).Services = app.Services;

        app.Start();

        //var host = Host.CreateDefaultBuilder()
        //    .ConfigureHostConfiguration(config => { })
        //    .ConfigureAppConfiguration((context, config) => config
        //        .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)))
        //    .ConfigureServices((context, services) =>
        //    {
        //        services.AddSimpleInjector(container, options =>
        //        {
        //            options.DisposeContainerWithServiceProvider = false;

        //            options.AddLogging();
        //        });

        //        ConfigureServices(services);
        //    })
        //    .ConfigureLogging((context, config) => config
        //        .AddLog4Net())
        //    .Build();

        //host.UseSimpleInjector(container);

        //((MonacoApplication)Application.Current).Services = host.Services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILobby, LobbyEngine>();

        services.AddSingleton<IGameLoader, GameLoader>();

        services.AddSingleton<ILayoutManager, LayoutManager>();

        services.AddTransient<IScreenMapper, ScreenMapper>();

        services.AddSingleton<IRegionManager, RegionManager>();

        services.AddSingleton<IRegionViewRegistry, RegionViewRegistry>();

        services.AddTransient<IRegionNavigator, RegionNavigator>();

        services.AddTransient<DelayedRegionCreationStrategy>();

        services.AddTransient(typeof(IRegionCreator<>), typeof(RegionCreator<>));

        services.AddSingleton(
            typeof(RegionAdapter<>).MakeGenericType(typeof(ContentControl)),
            typeof(ContentControlRegionAdapter));

        services.AddSingleton<IRegionAdapterMapper, RegionAdapterMapper>();
        services.AddSingleton<CreateRegionAdapter>(
            provider => elementType => CreatRegionAdapterFactory(provider, elementType));

        services.AddSingleton<IApplicationCommands, ApplicationCommands>();

        services.AddSingleton<ICommandHandlerFactory, CommandHandlerFactory>();
        services.AddManyForOpenGeneric(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());

        services.AddSingleton(
            sp => sp.GetRequiredService<IPropertiesManager>()
                      .GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null) ??
                  throw new InvalidOperationException("Lobby configuration not found"));

        services.AddFluxor(
            options => options
                .WithLifetime(StoreLifetime.Singleton)
                .ScanAssemblies(Assembly.GetExecutingAssembly())
                .UseSelectors()
                .AddMiddleware<RehydrateGamesMiddleware>()
            //.UseRemoteReduxDevTools(
            //    devToolsOptions =>
            //    {
            //        devToolsOptions.RemoteReduxDevToolsUri = new Uri("https://localhost:7232/clientapphub");
            //        devToolsOptions.RemoteReduxDevToolsSessionId = "71637a4c-43b7-4ab0-a658-15b85e3c037f";
            //        devToolsOptions.Name = "Monaco Lobby";
            //        //devToolsOptions.EnableStackTrace();
            //    });
        );

        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<IAttractService, AttractService>();
        services.AddSingleton<IEdgeLightingService, EdgeLightingService>();
        services.AddSingleton<ITranslateService, TranslateService>();
        services.AddSingleton<IUpiService, UpiService>();
        services.AddSingleton<Services.Attendant.IAttendantService, AttendantService>();
        services.AddSingleton<IBankService, BankService>();

        services.AddSingleton(_ => ServiceManager.GetInstance().GetService<IWpfWindowLauncher>());

        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IPropertiesManager>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IEventBus>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<IOperatorMenuLauncher>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IAudio>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IGameOrderSettings>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IGameStorage>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<IProgressiveConfigurationProvider>());
        services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<ILocalizerFactory>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<IEdgeLightingStateManager>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<IAttractConfigurationProvider>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<ICabinetDetectionService>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<Contracts.IAttendantService>());
        services.AddSingleton(
            provider => provider.GetRequiredService<Container>().GetInstance<IBank>());
    }

    private static IRegionAdapter CreatRegionAdapterFactory(IServiceProvider provider, Type elementType)
    {
        var currentType = elementType;

        while (currentType != null)
        {
            if (provider.GetService(typeof(RegionAdapter<>).MakeGenericType(currentType)) is IRegionAdapter adapter)
            {
                return adapter;
            }

            currentType = currentType.BaseType;
        }

        throw new InvalidOperationException($"No region adapter found for {elementType} type");
    }
}
