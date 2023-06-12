namespace Aristocrat.Monaco.Gaming.VideoLottery
{
    using System;
    using System.Configuration;
    using System.Linq;
    using Application.Contracts.Media;
    using Common.Container;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.PlayerInfoDisplay;
    using Kernel;
    using ScreenSaver;
    using SimpleInjector;
    using UI;
    using UI.CompositionRoot;
    using UI.PlayerInfoDisplay;
    using UI.Views.Lobby;
    using UI.Views.MediaDisplay.Handlers;

    [CLSCompliant(false)]
    public sealed class VideoLotteryRunnable : GamingRunnable
    {
        protected override void ConfigureContainer(Container container)
        {
            container.Register<ILobby, LobbyLauncher>(Lifestyle.Singleton);
            container.Register<ILobbyStateManager, LobbyStateManager>(Lifestyle.Singleton);
            container.Register<IBrowserProcessManager, BrowserProcessManager>(Lifestyle.Singleton);
            container.AddOverlayMessageStrategies();

            // RG based properties
            container.Collection.Append<IPropertyProvider, PropertyProvider>(Lifestyle.Singleton);

            container.Register<IResponsibleGaming, ResponsibleGaming>(Lifestyle.Singleton);

            // we have to register in UI level dues to PlayerInfoDisplayManagerFactory and PlayerInfoDisplayFeatureProvider
            // and we want to avoid Aristocrat.Monaco.Gaming
            container.Register<IPlayerInfoDisplayManagerFactory, PlayerInfoDisplayManagerFactory>(Lifestyle.Singleton);
            container.Register<IPlayerInfoDisplayFeatureProvider, PlayerInfoDisplayFeatureProvider>(Lifestyle.Singleton);
            container.Register<IGameResourcesModelProvider, GameResourcesModelProvider>(Lifestyle.Singleton);

            container.Register<ScreenSaverMonitor>(Lifestyle.Singleton);
            container.Register<ScreenSaverWindowLauncher>(Lifestyle.Singleton);

            container.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                typeof(Consumers.Consumes<>).Assembly);
        }

        protected override void LoadUi(Container container)
        {
            var properties = container.GetInstance<IPropertiesManager>();

            properties.SetProperty(GamingConstants.MarketType, MarketType.VideoLottery);

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
