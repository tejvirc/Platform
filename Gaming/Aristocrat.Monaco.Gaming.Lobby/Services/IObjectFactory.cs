namespace Aristocrat.Monaco.Gaming.Lobby.Services;

public interface IObjectFactory
{
    T GetObject<T>() where T : class;
}
