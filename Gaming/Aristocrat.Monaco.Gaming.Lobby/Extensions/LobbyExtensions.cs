namespace Aristocrat.Monaco.Gaming.Lobby;

using SimpleInjector;

public static class LobbyExtensions
{
    public static Container RegisterLobby(this Container container)
    {
        container.Register<ILobby>(() =>
        {
            var config = container.GetInstance<LobbyConfiguration>();

            // TODO Check config to determine if using the default lobby or game-driven lobby

            return container.GetInstance<DefaultLobbyController>();
        }, Lifestyle.Singleton);

        return container;
    }
}
