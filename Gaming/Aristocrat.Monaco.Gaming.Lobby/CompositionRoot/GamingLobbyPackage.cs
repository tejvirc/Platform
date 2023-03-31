namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System.Windows;
using Consumers;
using Kernel;
using Lobby;
using ResponsibleGaming;
using Services;
using SimpleInjector;
using SimpleInjector.Packaging;
using UI.Common;
using ViewModels;

public class GamingLobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<Contracts.Lobby.ILobby, LobbyLauncher>(Lifestyle.Singleton);
        container.Register<IViewCollection, ViewCollection>(Lifestyle.Singleton);

        container.Register<IGameLoader, GameLoader>(Lifestyle.Singleton);

        container.Register<DefaultLobbyViewModel>(Lifestyle.Transient);
        container.Register<ChooserViewModel>(Lifestyle.Transient);

        container.RegisterLobby();

        container.RegisterResponsibleGaming();

        container.RegisterConsumers();

        container.RegisterFluxor();

        container.RegisterInstance(ServiceManager.GetInstance().GetService<IWpfWindowLauncher>());

        ((MonacoApplication)Application.Current).Services = container;
    }
}
