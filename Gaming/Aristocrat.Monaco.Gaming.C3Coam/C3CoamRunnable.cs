namespace Aristocrat.Monaco.Gaming.C3Coam
{
    using System.Configuration;
    using System.Linq;
    using Accounting.Contracts.HandCount;
    using Application.Contracts.Media;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Common.Container;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.PlayerInfoDisplay;
    using Kernel;
    using SimpleInjector;
    using UI;
    using UI.CompositionRoot;
    using UI.PlayerInfoDisplay;
    using UI.Views.Lobby;
    using UI.Views.MediaDisplay.Handlers;

    public  class C3CoamRunnable : GamingRunnable
    {
        protected override void ConfigureContainer(Container container)
        {
            container.Register<ILobby, LobbyLauncher>(Lifestyle.Singleton);
            container.Register<ILobbyStateManager, LobbyStateManager>(Lifestyle.Singleton);
            container.Register<IBrowserProcessManager, BrowserProcessManager>(Lifestyle.Singleton);
            container.AddOverlayMessageStrategies();

            // we have to register in UI level dues to PlayerInfoDisplayManagerFactory and PlayerInfoDisplayFeatureProvider
            // and we want to avoid Aristocrat.Monaco.Gaming
            container.Register<IPlayerInfoDisplayManagerFactory, PlayerInfoDisplayManagerFactory>(Lifestyle.Singleton);
            container.Register<IPlayerInfoDisplayFeatureProvider, PlayerInfoDisplayFeatureProvider>(Lifestyle.Singleton);
            container.Register<IGameResourcesModelProvider, GameResourcesModelProvider>(Lifestyle.Singleton);

            // This is just a stub since the core gaming layer currently has dependencies on IResponsibleGaming.  It needs to be re-factored out
            container.Register<IResponsibleGaming, ResponsibleGaming>(Lifestyle.Singleton);

            container.Register<IPlayerBank, CoamPlayerBank>(Lifestyle.Singleton);
            container.Register(() => ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(), Lifestyle.Singleton);

            // Additional registrations go here

            container.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                typeof(Consumers.Consumes<>).Assembly);
        }

        protected override void LoadUi(Container container)
        {
            var properties = container.GetInstance<IPropertiesManager>();

            var lobbyStateManager = container.GetInstance<ILobbyStateManager>();
            lobbyStateManager.IsTabView = true;
            lobbyStateManager.ResetAttractOnInterruption = true;
            lobbyStateManager.AllowGameInCharge = (bool)properties.GetProperty(GamingConstants.AllowGameInCharge, false);

            // This info needs to be available before the lobby is loaded
            LoadLobbyConfig(container);

            var lobby = container.GetInstance<ILobby>();
            if (RunState == RunnableState.Running)
            {
                lobby.CreateWindow();
            }
        }

        protected override void UnloadUi(Container container)
        {
            var lobby = container.GetInstance<ILobby>();

            lobby.Close();
        }

        private static void LoadLobbyConfig(Container container)
        {
            var config = ConfigurationUtilities.GetConfiguration<LobbyConfiguration>(
                "/Lobby/Configuration",
                () => throw new ConfigurationErrorsException());

            if (config != null)
            {
                config.LocaleCodes = config.LocaleCodes.Select(s => s.ToUpperInvariant()).ToArray();
                if (config.ResponsibleGamingInfo == null)
                {
                    config.ResponsibleGamingInfo = new ResponsibleGamingInfoOptions();
                }
            }

            container.GetInstance<IPropertiesManager>().SetProperty(GamingConstants.LobbyConfig, config);
        }
    }
}
