namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System.Windows;
using System.Windows.Controls;
using Consumers;
using Contracts.Lobby;
using Kernel;
using Redux;
using Regions;
using Services;
using Services.Layout;
using Services.OperatorMenu;
using Services.ResponsibleGaming;
using SimpleInjector;
using SimpleInjector.Packaging;
using Store.Chooser;
using Store.Lobby;
using UI.Common;

public class GamingLobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<ILobby, LobbyController>(Lifestyle.Singleton);

        container.Register<IGameLoader, GameLoader>(Lifestyle.Singleton);

        container.Register<ILayoutManager, LayoutManager>(Lifestyle.Singleton);

        container.Register<IOperatorMenuController, OperatorMenuController>(Lifestyle.Singleton);

        container.Register<IScreenMapper, ScreenMapper>();

        container.Register<IRegionManager, RegionManager>(Lifestyle.Singleton);

        container.Register<IRegionViewRegistry, RegionViewRegistry>(Lifestyle.Singleton);

        container.Register<IRegionNavigator, RegionNavigator>();

        container.Register<DelayedRegionCreationStrategy>();

        container.Register(typeof(IRegionCreator<>), typeof(RegionCreator<>), Lifestyle.Transient);

        var regionAdapterMapper = new RegionAdapterMapper(container);
        regionAdapterMapper.Register<ContentControlRegionAdapter>(typeof(ContentControl));

        container.RegisterInstance<IRegionAdapterMapper>(regionAdapterMapper);

        container.Register(typeof(IStateLens<>), typeof(StateLens<>), Lifestyle.Singleton);

        container.RegisterLobby();

        container.RegisterResponsibleGaming();

        container.RegisterConsumers();

        container.RegisterFluxor();

        container.RegisterInstance(ServiceManager.GetInstance().GetService<IWpfWindowLauncher>());

        ((MonacoApplication)Application.Current).Services = container;
    }
}
