﻿namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using System.Reflection;
using Accounting.Contracts;
using Application.Contracts.EdgeLight;
using Application.Contracts.Localization;
using Contracts;
using Contracts.Lobby;
using Extensions.Fluxor;
using Fluxor;
using Hardware.Contracts.Audio;
using Hardware.Contracts.Cabinet;
using Kernel;
using Lobby.Commands;
using Lobby.Services.Attendant;
using Log4Net.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Monaco.UI.Common;
using Progressives;
using ReactiveUI;
using Services;
using Services.Attract;
using Services.Audio;
using Services.Bank;
using Services.EdgeLighting;
using Services.Translate;
using Services.Upi;
using SimpleInjector;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Store.Middleware;
using Vgt.Client12.Application.OperatorMenu;

public class ReactiveUIBootstrapper
{
    private IServiceProvider? Container { get; set; }

    public void InitializeContainer(Container container)
    {
        var services = new ServiceCollection();

        services.AddSimpleInjector(container, options =>
        {
            options.DisposeContainerWithServiceProvider = false;
        });

        services.UseMicrosoftDependencyResolver();

        services.AddLogging(builder => builder.AddLog4Net());
        services.AddLogging(builder => builder.AddSplat());

        ConfigureServices(services);

        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        Container = services
            .BuildServiceProvider(validateScopes: true)
            .UseSimpleInjector(container);

        Container.UseMicrosoftDependencyResolver();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILobby, LobbyEngine>();

        services.AddSingleton<IGameLoader, GameLoader>();

        services.AddSingleton<ILayoutManager, LayoutManager>();

        services.AddTransient<IScreenMapper, ScreenMapper>();

        services.AddSingleton<IApplicationCommands, ApplicationCommands>();

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
        services.AddSingleton<IAttendantAgent, AttendantAgent>();
        services.AddSingleton<IAttendant, AttendantService>();
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
}
