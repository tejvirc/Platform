namespace Aristocrat.Monaco.Gaming.Class3
{
    using System.Configuration;
    using System.Linq;
    using Application.Contracts.Media;
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

    public sealed class Class3Runnable : GamingRunnable
    {
        protected override void ConfigureContainer(Container container)
        {
            // container.Register<ILobby, LobbyLauncher>(Lifestyle.Singleton);
            // container.Register<ILobbyStateManager, LobbyStateManager>(Lifestyle.Singleton);
            container.Register<IBrowserProcessManager, BrowserProcessManager>(Lifestyle.Singleton);
            container.AddOverlayMessageStrategies();

            // we have to register in UI level dues to PlayerInfoDisplayManagerFactory and PlayerInfoDisplayFeatureProvider
            // and we want to avoid Aristocrat.Monaco.Gaming
            container.Register<IPlayerInfoDisplayManagerFactory, PlayerInfoDisplayManagerFactory>(Lifestyle.Singleton);
            container.Register<IPlayerInfoDisplayFeatureProvider, PlayerInfoDisplayFeatureProvider>(Lifestyle.Singleton);
            container.Register<IGameResourcesModelProvider, GameResourcesModelProvider>(Lifestyle.Singleton);

            // This is just a stub since the core gaming layer currently has dependencies on IResponsibleGaming.  It needs to be re-factored out
            container.Register<IResponsibleGaming, ResponsibleGaming>(Lifestyle.Singleton);

            // Additional registrations go here

            container.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                typeof(Consumers.Consumes<>).Assembly);
        }


        protected override void LoadUi(Container container)
        {
            var lobbyStateManager = container.GetInstance<ILobbyStateManager>();
            lobbyStateManager.IsTabView = true;
            lobbyStateManager.ResetAttractOnInterruption = true;
            lobbyStateManager.AllowGameInCharge = true;

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
                config.ResponsibleGamingInfo ??= new ResponsibleGamingInfoOptions();
            }

            container.GetInstance<IPropertiesManager>().SetProperty(GamingConstants.LobbyConfig, config);
        }
    }
}
