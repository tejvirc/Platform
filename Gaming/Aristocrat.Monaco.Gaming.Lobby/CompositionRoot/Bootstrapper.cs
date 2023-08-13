namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Accounting.Contracts;
using Application.Contracts.EdgeLight;
using Application.Contracts.Localization;
using Commands;
using Contracts;
using Contracts.Lobby;
using Extensions.Fluxor;
using Extensions.Prism;
using Fluxor;
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

        _containerExtension.Services.AddLogging(builder => builder.AddLog4Net());

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
        services.AddSingleton<ILobby, LobbyService>();

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
