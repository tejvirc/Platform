namespace Aristocrat.Monaco.Gaming.Lobby.Controllers;

public interface IControllerFactory
{
    TController GetController<TController>() where TController : class, IController;
}
