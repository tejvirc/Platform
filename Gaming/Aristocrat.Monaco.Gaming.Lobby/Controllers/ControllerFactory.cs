namespace Aristocrat.Monaco.Gaming.Lobby.Controllers;

using SimpleInjector;

public class ControllerFactory : IControllerFactory
{
    private readonly Container _container;

    public ControllerFactory(Container container)
    {
        _container = container;
    }

    public TController GetController<TController>() where TController : class, IController
    {
        return _container.GetInstance<TController>();
    }
}
