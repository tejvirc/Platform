namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;
using CommandHandlers;
using Commands;
using Common.Container;
using Consumers;
using Contracts.Lobby;
using Fluxor.Extensions;
using Kernel;
using Regions;
using Runtime;
using Runtime.Server;
using Services;
using SimpleInjector;
using SimpleInjector.Packaging;
using UI.Common;

public class GamingLobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<ILobby, LobbyEngine>(Lifestyle.Singleton);

        container.Register<ILobbyService, LobbyService>(Lifestyle.Singleton);

        container.Register<IGameLoader, GameLoader>(Lifestyle.Singleton);

        container.Register<ILayoutManager, LayoutManager>(Lifestyle.Singleton);

        container.Register<IOperatorMenuController, OperatorMenuController>(Lifestyle.Singleton);

        container.Register<IScreenMapper, ScreenMapper>();

        container.Register<IRegionManager, RegionManager>(Lifestyle.Singleton);

        container.Register<IRegionViewRegistry, RegionViewRegistry>(Lifestyle.Singleton);

        container.Register<IRegionNavigator, RegionNavigator>(Lifestyle.Transient);

        container.Register<DelayedRegionCreationStrategy>(Lifestyle.Transient);

        container.Register(typeof(IRegionCreator<>), typeof(RegionCreator<>), Lifestyle.Transient);

        var regionAdapterMapper = new RegionAdapterMapper(container);
        regionAdapterMapper.Register<ContentControlRegionAdapter>(typeof(ContentControl));

        container.RegisterInstance<IRegionAdapterMapper>(regionAdapterMapper);

        container.Register<IApplicationCommands, ApplicationCommands>(Lifestyle.Singleton);

        container.Register<IObjectFactory, ObjectFactory>(Lifestyle.Singleton);

        container.Register<ICommandHandlerFactory, CommandHandlerFactory>(Lifestyle.Singleton);
        container.RegisterManyForOpenGeneric(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());

        container.RegisterConsumers();

        container.RegisterFluxor();

        container.RegisterInstance(ServiceManager.GetInstance().GetService<IWpfWindowLauncher>());

        container.RegisterSingleton(
            () => container.GetInstance<IPropertiesManager>()
                      .GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null) ??
                  throw new InvalidOperationException("Lobby configuration not found"));

        ((MonacoApplication)Application.Current).Services = container;
    }
}
