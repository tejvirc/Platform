namespace Aristocrat.Monaco.Gaming.Presentation.CompositionRoot;

using SimpleInjector;
using SimpleInjector.Packaging;

public class LobbyPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        var bootstrapper = new Bootstrapper();
        bootstrapper.InitializeContainer(container);
    }
}
