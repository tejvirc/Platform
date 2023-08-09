namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using SimpleInjector;
using SimpleInjector.Packaging;

public class LobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        var bootstrapper = new ReactiveUIBootstrapper();
        bootstrapper.InitializeContainer(container);
    }
}
