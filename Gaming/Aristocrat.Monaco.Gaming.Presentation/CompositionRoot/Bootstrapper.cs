namespace Aristocrat.Monaco.Gaming.Presentation.CompositionRoot;

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Accounting.Contracts;
using Application.Contracts.EdgeLight;
using Application.Contracts.Localization;
using Commands;
using Extensions.Fluxor;
using Extensions.Prism;
using Fluxor;
using Gaming.Commands;
using Gaming.Contracts;
using Gaming.Contracts.Lobby;
using Hardware.Contracts.Audio;
using Hardware.Contracts.Cabinet;
using Kernel;
using Log4Net.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Monaco.UI.Common;
using Prism;
using Prism.Ioc;
using Prism.Mvvm;
using Progressives;
using Services;
using Services.Attendant;
using Services.Attract;
using Services.Audio;
using Services.Bank;
using Services.EdgeLighting;
using Services.Replay;
using Services.Translate;
using Services.Upi;
using SimpleInjector;
using Store.Middleware;
using Vgt.Client12.Application.OperatorMenu;
using Views;

public sealed class Bootstrapper : PrismBootstrapperBase
{
    private readonly PrismContainerExtension _containerExtension;

    private Container? _container;

    public Bootstrapper()
    {
        _containerExtension = new PrismContainerExtension(new ServiceCollection());
    }

    public void InitializeContainer(Container container)
    {
        _container = container;

        _containerExtension.Services.AddSimpleInjector(container, options =>
        {
            options.DisposeContainerWithServiceProvider = false;
        });

        Run();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Container.Resolve<IServiceProvider>().UseSimpleInjector(_container!);
    }

    protected override DependencyObject? CreateShell()
    {
        // Delay Shell window creation
        return null;
    }

    protected override IContainerExtension CreateContainerExtension() => _containerExtension;

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();

        ViewModelLocationProvider.SetDefaultViewModelFactory(
            (view, type) => ActivatorUtilities.CreateInstance(Container.Resolve<IServiceProvider>(), type));

        ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
        {
            var viewName = viewType.Name;
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName!;
            var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
            var ns = $"{viewType.Namespace![..^"Views".Length]}ViewModels";
            var viewModelName = string.Format(CultureInfo.InvariantCulture, $"{ns}.{viewName}{suffix}, {viewAssemblyName}");
            return Type.GetType(viewModelName);
        });
    }

    protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
    {
        base.RegisterRequiredTypes(containerRegistry);
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<LobbyMainView>(ViewNames.Lobby);
        containerRegistry.RegisterForNavigation<AttractMainView>(ViewNames.Attract);
        containerRegistry.RegisterForNavigation<LoadingMainView>(ViewNames.Loading);
        containerRegistry.RegisterForNavigation<ChooserView>(ViewNames.Chooser);
        containerRegistry.RegisterForNavigation<BannerView>(ViewNames.Banner);
        containerRegistry.RegisterForNavigation<StandardUpiView>(ViewNames.StandardUpi);
        containerRegistry.RegisterForNavigation<MultiLanguageUpiView>(ViewNames.MultiLingualUpi);
        containerRegistry.RegisterForNavigation<InfoBarView>(ViewNames.InfoBar);
        containerRegistry.RegisterForNavigation<ReplayNavView>(ViewNames.ReplayNav);

        containerRegistry.RegisterServices(ConfigureServices);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddLog4Net());

        services.AddLobbyConfigurationOptions();

        services.AddSingleton<ILobby, PresentationLauncher>();

        services.AddSingleton<IPresentationService, PresentationService>();

        services.AddLobbyConfigurationOptions();

        services.AddSingleton<IGameLoader, GameLoader>();

        services.AddSingleton<ILayoutManager, LayoutManager>();

        services.AddTransient<IScreenMapper, ScreenMapper>();

        services.AddSingleton<IApplicationCommands, ApplicationCommands>();

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
        services.AddSingleton<ITranslateAgent, TranslateAgent>();
        services.AddSingleton<IUpiService, UpiService>();
        services.AddSingleton<Services.Attendant.IAttendantService, AttendantService>();
        services.AddSingleton<Services.Attendant.IAttendantService, AttendantService>();
        services.AddSingleton<IBankService, BankService>();
        services.AddSingleton<IReplayService, ReplayService>();
        services.AddSingleton<IOperatorMenuService, OperatorMenuService>();
        services.AddSingleton<IGameLauncher, GameLauncher>();

        services.AddLocatedPlatformService<IWpfWindowLauncher>();
        services.AddPlatformService<IPropertiesManager>();
        services.AddPlatformService<IPropertiesManager>();
        services.AddPlatformService<IEventBus>();
        services.AddPlatformService<IOperatorMenuLauncher>();
        services.AddPlatformService<IAudio>();
        services.AddPlatformService<IGameOrderSettings>();
        services.AddPlatformService<IGameStorage>();
        services.AddPlatformService<IProgressiveConfigurationProvider>();
        services.AddPlatformService<ILocalizerFactory>();
        services.AddPlatformService<IEdgeLightingStateManager>();
        services.AddPlatformService<IAttractConfigurationProvider>();
        services.AddPlatformService<ICabinetDetectionService>();
        services.AddPlatformService<Gaming.Contracts.IAttendantService>();
        services.AddPlatformService<IBank>();
        services.AddPlatformService<ICommandHandlerFactory>();
        services.AddPlatformService<IGameDiagnostics>();
    }
}
